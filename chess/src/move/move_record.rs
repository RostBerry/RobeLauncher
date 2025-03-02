use super::Move;

/// Contains all information needed to undo a move
pub struct MoveRecord {
    pub mov: Move,
    pub captured_piece_type: usize,
    /// The previous en passant state
    pub is_en_passant_possible: bool,
    pub en_passant_pawn_square: usize,
    pub en_passant_capture_square: usize,
    /// The previous castling states for both players
    pub old_castling_states: [u8; 2],
}

impl MoveRecord {
    pub fn new(
        mov: Move,
        captured_piece_type: usize,
        is_en_passant_possible: bool,
        en_passant_pawn_square: usize,
        en_passant_capture_square: usize,
        old_castling_states: [u8; 2],
    ) -> Self {
        Self {
            mov,
            captured_piece_type,
            is_en_passant_possible,
            en_passant_pawn_square,
            en_passant_capture_square,
            old_castling_states,
        }
    }
}