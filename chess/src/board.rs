use crate::{bitboards, board_representation, castling, r#move::{move_record::MoveRecord, Move, MoveType}, piece::{self, *}};

// constants
/// Since the squares on the board go from 0 to 63 included, 64 is out of bounds making it easier to catch things like king abscense
pub const INVALID_SQUARE: usize = 64;

const QUEEN_SIDE_CASTLING_ROOK_SQUARE: [usize; 2] = [7, 63];
const KING_SIDE_CASTLING_ROOK_SQUARE: [usize; 2] = [0, 56];

fn get_king_side_castling_rook_square(color: usize) -> usize {
    debug_assert!(color < 2, "Color is out of bounds");
    unsafe {
        *KING_SIDE_CASTLING_ROOK_SQUARE.get_unchecked(color)
    }
}

fn get_queen_side_castling_rook_square(color: usize) -> usize {
    debug_assert!(color < 2, "Color is out of bounds");
    unsafe {
        *QUEEN_SIDE_CASTLING_ROOK_SQUARE.get_unchecked(color)
    }
}

const KING_START_SQUARES: [usize; 2] = [3, 59];

pub fn get_king_start_square(color: usize) -> usize {
    debug_assert!(color < 2, "Color is out of bounds");
    unsafe {
        *KING_START_SQUARES.get_unchecked(color)
    }
}

const KING_SIDE_CASTLING_SQUARES: [usize; 2] = [1, 57];
const QUEEN_SIDE_CASTLING_SQUARES: [usize; 2] = [5, 61];

pub fn get_king_side_square(color: usize) -> usize {
    debug_assert!(color < 2, "Color is out of bounds");
    unsafe {
        *KING_SIDE_CASTLING_SQUARES.get_unchecked(color)
    }
}

pub fn get_queen_side_square(color: usize) -> usize {
    debug_assert!(color < 2, "Color is out of bounds");
    unsafe {
        *QUEEN_SIDE_CASTLING_SQUARES.get_unchecked(color)
    }
}

const CASTLED_KING_SIDE_ROOK_SQUARES: [usize; 2] = [2, 58];
const CASTLED_QUEEN_SIDE_ROOK_SQUARES: [usize; 2] = [4, 60];

pub fn get_castled_king_side_rook_square(color: usize) -> usize {
    debug_assert!(color < 2, "Color is out of bounds");
    unsafe {
        *CASTLED_KING_SIDE_ROOK_SQUARES.get_unchecked(color)
    }
}

pub fn get_castled_queen_side_rook_square(color: usize) -> usize {
    debug_assert!(color < 2, "Color is out of bounds");
    unsafe {
        *CASTLED_QUEEN_SIDE_ROOK_SQUARES.get_unchecked(color)
    }
}

const EMPTY_SQUARES: [(usize, usize); 64] = [(piece::INVALID_COLOR, piece::NONE); 64];
const EMPTY_PIECES: [[u64; 7]; 2] = unsafe { std::mem::zeroed() };
const DEFAULT_CASTLING_STATES: [u8; 2] = [0b11, 0b11];

/// Contains everything about the current position
pub struct Board {
    ///Contains bitboards for every piece type for each color
    /// 
    /// The bitboards at index 0 are for all pieces combined
    pieces: [[u64; 7]; 2],
    squares: [(usize, usize); 64],
    ///Can be either *WHITE* or *BLACK*
    /// 
    /// It is integer instead of bool because it is used in array indexing like in *king_square* or *piece*
    current_color: usize,
    castling_states: [u8; 2],
    is_en_passant_possible: bool,
    en_passant_pawn_square: usize,
    en_passant_capture_square: usize,
}

impl Board {
    /// Creates empty board
    pub fn new () -> Self {
        Self {
            pieces: EMPTY_PIECES,
            squares: EMPTY_SQUARES,
            current_color: WHITE,
            castling_states: DEFAULT_CASTLING_STATES,
            is_en_passant_possible: false,
            en_passant_pawn_square: INVALID_SQUARE,
            en_passant_capture_square: INVALID_SQUARE,
        }
    }

    /// Creates board from a position in the provided FEN string
    pub fn from_fen(fen_string: &str) -> Self {
        let fen_data: Vec<&str> = fen_string.split(" ").collect();

        let mut board = Self::new();
        board.load_position(fen_data[0]);

        if fen_data[1] == "b" {
            board.switch_color();
        }

        for i in 0..2 {
            let can_short = fen_data[2].contains(if i == 0 {'K'} else {'k'});
            let can_long = fen_data[2].contains(if i == 0 {'Q'} else {'q'});

            board.castling_states[i] = 0b11;

            if !can_short {
                castling::annul_king_side(&mut board.castling_states[i]);
            }
            if !can_long {
                castling::annul_queen_side(&mut board.castling_states[i]);
            }
        }

        board
    }

    fn load_position(&mut self, fen_pos: &str) {
        let rows: Vec<&str> = fen_pos.split("/").collect();

        for y in 0..8usize {
            let mut x = 7i8;

            for sym in rows[7 - y].chars() {
                if x < 0 {
                    continue;
                }

                if sym.is_digit(10) {
                    x -= sym.to_digit(10).expect("FEN loading") as i8;
                    continue;
                }

                let (color, piece_type) = board_representation::get_piece_from_fen(&sym);
                let square = x as usize + y * 8;
                if *piece_type != piece::NONE {
                    self.create_piece(square, *color, *piece_type);

                }
                x -= 1;
            }
        }
    }

    /// Returns the FEN string of the current position
    pub fn to_fen(&mut self) -> String {
        let mut fen_string = String::new();

        for y in (0..8).rev() {
            let mut empty_squares = 0;

            for x in (0..8).rev() {
                let square = x + y * 8;
                let (color, piece_type) = self.get_piece_on_square(square);

                if piece_type == piece::NONE {
                    empty_squares += 1;
                } else {
                    if empty_squares > 0 {
                        fen_string.push_str(&empty_squares.to_string());
                        empty_squares = 0;
                    }

                    fen_string.push(board_representation::piece_to_fen_sym(color, piece_type));
                }
            }

            if empty_squares > 0 {
                fen_string.push_str(&empty_squares.to_string());
            }

            if y > 0 {
                fen_string.push('/');
            }
        }

        fen_string.push(' ');

        fen_string.push(if self.is_white_to_move() {'w'} else {'b'});
        fen_string.push(' ');

        let mut castling_string = String::new();
        if castling::can_king_side(self.castling_states[WHITE]) {
            castling_string.push('K');
        }
        if castling::can_queen_side(self.castling_states[WHITE]) {
            castling_string.push('Q');
        }
        if castling::can_king_side(self.castling_states[BLACK]) {
            castling_string.push('k');
        }
        if castling::can_queen_side(self.castling_states[BLACK]) {
            castling_string.push('q');
        }
        if castling_string.is_empty() {
            castling_string.push('-');
        }
        fen_string.push_str(&castling_string);

        fen_string.push(' ');

        if self.is_en_passant_possible() {
            fen_string.push_str(
                &board_representation::get_square_name(
                    self.en_passant_pawn_square()
                )
            );
        } else {
            fen_string.push('-');
        }

        fen_string
    }

    pub fn get_current_color(&self) -> usize {
        self.current_color
    }

    pub fn get_opposite_color(&self) -> usize {
        1 - self.current_color
    }

    /// Returns the square of the king of the color provided
    pub fn get_king_square(&self, color: usize) -> usize {
        debug_assert!(color < 2, "Color is out of bounds");
        debug_assert!(self.get_piece_bitboard(color, piece::KING).count_ones() == 1, "Wrong number of kings");
        unsafe {
            self.pieces.get_unchecked(color).get_unchecked(piece::KING).trailing_zeros() as usize
        }
    }

    /// Returns bitboard corresponding to the provided piece
    pub fn get_piece_bitboard(&self, color: usize, piece_type: usize) -> u64 {
        debug_assert!(piece_type < 7, "Piece type is out of bounds");
        debug_assert!(color < 2, "Color is out of bounds");
        unsafe {
            *self.pieces.get_unchecked(color).get_unchecked(piece_type)
        }
    }

    /// Returns piece standing on the provided square (or *INVALID_PIECE* if the square is empty)
    pub fn get_piece_on_square(&self, square: usize) -> (usize, usize) {
        debug_assert!(square < 64, "Square is out of bounds");
        unsafe {
            *self.squares.get_unchecked(square)
        }
    }

    /// Returns bitboard containing every single piece on the board
    pub fn get_all_occupied_squares(&self) -> u64 {
        self.get_all_occupied_squares_for_color(WHITE) | self.get_all_occupied_squares_for_color(BLACK)
    }

    /// Returns bitboard containing every single piece of the color provided
    pub fn get_all_occupied_squares_for_color(&self, color: usize) -> u64 {
        debug_assert!(color < 2, "Color is out of bounds");
        unsafe {
            *self.pieces.get_unchecked(color).get_unchecked(0)
        }
    }
    
    pub fn is_white_to_move(&self) -> bool {
        self.get_current_color() == WHITE
    }

    ///Returns the castling state for the side of the color provided
    pub fn get_castling_state(&mut self, color: usize) -> &mut u8 {
        debug_assert!(color < 2, "Color is out of bounds");
        unsafe {
            &mut *self.castling_states.get_unchecked_mut(color)
        }
    }

    pub fn is_en_passant_possible(&self) -> bool {
        self.is_en_passant_possible
    }

    pub fn en_passant_pawn_square(&self) -> usize {
        self.en_passant_pawn_square
    }

    pub fn en_passant_capture_square(&self) -> usize {
        self.en_passant_capture_square
    }

    /// Updates the en passant state based on the provided square
    fn update_en_passant_state(&mut self, possible: bool, pawn_square: usize, capture_square: usize) {
        self.is_en_passant_possible = possible;
        if !possible {
            return;
        }

        self.en_passant_pawn_square = pawn_square;
        self.en_passant_capture_square = capture_square;
    }

    /// Used after every move
    pub fn switch_color(&mut self) {
        self.current_color = 1 - self.current_color;
    }

    fn delete_piece(&mut self, square: usize) {
        debug_assert!(square < 64, "Square is out of bounds");
        let inverted_bit: u64 = !bitboards::get_bit_from_square(square);

        unsafe {
            let (color, piece_type) = self.squares.get_unchecked_mut(square);
        
            *self.pieces.get_unchecked_mut(*color).get_unchecked_mut(0) &= inverted_bit;
            *self.pieces.get_unchecked_mut(*color).get_unchecked_mut(*piece_type) &= inverted_bit;

            *color = piece::INVALID_COLOR;
            *piece_type = piece::NONE;
        }
    }

    fn create_piece(&mut self, square: usize, color: usize, piece_type: usize) {
        debug_assert!(square < 64, "Square is out of bounds");
        debug_assert!(piece_type < 7, "Piece type is out of bounds");
        debug_assert!(color < 2, "Color is out of bounds");
        let bit = bitboards::get_bit_from_square(square);

        unsafe {
            let (current_color, current_piece_type) = self.squares.get_unchecked_mut(square);
            *current_color = color;
            *current_piece_type = piece_type;

            *self.pieces.get_unchecked_mut(color).get_unchecked_mut(0) |= bit;
            *self.pieces.get_unchecked_mut(color).get_unchecked_mut(piece_type) |= bit;
        }
    }

    pub fn make_move(&mut self, move_to_make: Move) -> MoveRecord {
        let old_castling_states = self.castling_states;

        let current_color = self.get_current_color();
        let opposite_color = self.get_opposite_color();

        let start_square = move_to_make.start_square;
        let target_square = move_to_make.target_square;

        let (_, mut piece_type) = self.get_piece_on_square(start_square);

        debug_assert!(piece_type != piece::NONE, "There is no piece on the start square");
        // deleting the piece from its start square
        self.delete_piece(start_square);

        let captured_square = move_to_make.capture_square;
        let (_, captured_piece_type) = self.get_piece_on_square(captured_square);

        if captured_piece_type != piece::NONE {
            self.delete_piece(captured_square);
        }

        self.update_en_passant_state(false, INVALID_SQUARE, INVALID_SQUARE);

        let king_side_castling_rook_square = get_king_side_castling_rook_square(current_color);
        let queen_side_castling_rook_square = get_queen_side_castling_rook_square(current_color);

        match move_to_make.move_type {
            MoveType::PromotionQueen => piece_type = piece::QUEEN,
            MoveType::PromotionKnight => piece_type = piece::KNIGHT,
            MoveType::PromotionRook => piece_type = piece::ROOK,
            MoveType::PromotionBishop => piece_type = piece::BISHOP,
            MoveType::PawnDoubleMove => self.update_en_passant_state(true, target_square, (start_square + target_square) / 2),
            MoveType::CastlingKingSide => {
                self.delete_piece(king_side_castling_rook_square);
                self.create_piece(get_castled_king_side_rook_square(current_color), current_color, piece::ROOK);
            },
            MoveType::CastlingQueenSide => {
                self.delete_piece(queen_side_castling_rook_square);
                self.create_piece(get_castled_queen_side_rook_square(current_color), current_color, piece::ROOK);
            },
            _ => (),
        }

        self.create_piece(target_square, current_color, piece_type);

        let mut castling_state = self.get_castling_state(current_color);
        if piece_type == KING {
            castling::annul(&mut castling_state);
        }

        if piece_type == ROOK {
            if start_square == king_side_castling_rook_square {
                castling::annul_king_side(&mut castling_state);
            }
            if start_square == queen_side_castling_rook_square {
                castling::annul_queen_side(&mut castling_state);
            }
        }

        let mut opposite_castling_state = self.get_castling_state(opposite_color);
        if target_square == get_king_side_castling_rook_square(opposite_color) {
            castling::annul_king_side(&mut opposite_castling_state);
        } 
        if target_square == get_queen_side_castling_rook_square(opposite_color) {
            castling::annul_queen_side(&mut opposite_castling_state);
        }

        self.switch_color();

        MoveRecord::new(
            move_to_make,
            captured_piece_type,
            self.is_en_passant_possible,
            self.en_passant_pawn_square,
            self.en_passant_capture_square,
            old_castling_states
        )
    }
    
    /// Undoes the move from the provided MoveInfo object
    pub fn undo_move(&mut self, move_record: MoveRecord) {
        let mov = move_record.mov;
        let start_square = mov.start_square;
        let target_square = mov.target_square;
        let captured_piece_type = move_record.captured_piece_type;
        let captured_square = mov.capture_square;
        self.castling_states = move_record.old_castling_states;
        self.is_en_passant_possible = move_record.is_en_passant_possible;
        self.en_passant_pawn_square = move_record.en_passant_pawn_square;
        self.en_passant_capture_square = move_record.en_passant_capture_square;

        let (_, mut moved_piece_type) = self.get_piece_on_square(target_square);

        debug_assert!(moved_piece_type != piece::NONE, "There is no piece on the target square");

        self.switch_color();

        let current_color = self.get_current_color();

        // deleting the piece from the target square
        self.delete_piece(target_square);


        match mov.move_type {
            MoveType::PromotionQueen | MoveType::PromotionKnight | MoveType::PromotionRook | MoveType::PromotionBishop => {
                moved_piece_type = piece::PAWN;
            },
            MoveType::CastlingKingSide => {
                self.delete_piece(get_castled_king_side_rook_square(current_color));
                self.create_piece(get_king_side_castling_rook_square(current_color), current_color, piece::ROOK);
            },
            MoveType::CastlingQueenSide => {
                self.delete_piece(get_castled_queen_side_rook_square(current_color));
                self.create_piece(get_queen_side_castling_rook_square(current_color), current_color, piece::ROOK);
            },
            _ => (),
        }

        self.create_piece(start_square, current_color, moved_piece_type);

        if captured_piece_type == piece::NONE {
            return;
        }

        self.create_piece(captured_square, self.get_opposite_color(), captured_piece_type, );
    }
}