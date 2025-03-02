use core::game::GameState;

use crate::{board::Board, board_representation, move_generation::{attack_calculator::AttackCalculator, move_gen}, piece, player::Player};

pub struct GameManager {
    board: Board,
    players: [Box<dyn Player>; 2],
    game_state: GameState
}

impl GameManager {
    pub fn new(players: [Box<dyn Player>; 2], fen_position: &str) -> Self {
        let board = Board::from_fen(fen_position);

        Self {
            board,
            players,
            game_state: GameState::InProgress
        }
    }

    pub fn run(&mut self) {
        loop {
            board_representation::print_board(&self.board);

            let attack_calculator = AttackCalculator::new(&self.board);
            let mut possible_moves = Vec::with_capacity(218);
            move_gen::generate_moves(&mut possible_moves, &mut self.board);

            if possible_moves.is_empty() {
                if attack_calculator.in_check() {
                    println!("Checkmate! {} wins!", self.board.get_current_color());
                    self.game_state = if self.board.get_current_color() == piece::WHITE { GameState::BlackWon } else { GameState::WhiteWon };
                } else {
                    println!("Stalemate!");
                    self.game_state = GameState::Draw;
                }
                break;
            }

            self.board.make_move(self.players[self.board.get_current_color()].get_move(&self.board));
        }
    }
}