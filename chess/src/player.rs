use std::io::{self, Write};

use crate::{board::Board, r#move::{Move, UciMove, UciMoveCreationResult}};

pub trait Player {
    fn get_move(&self, board: &Board) -> Move;
}

pub struct PlayerHuman {

}

impl Player for PlayerHuman {
    fn get_move(&self, board: &Board) -> Move {
        println!("Enter your move in UCI format (e.g., e2e4): ");
        io::stdout().flush().unwrap();

        let mut input = String::new();
        io::stdin().read_line(&mut input).unwrap();
        let input = input.trim();

        match UciMove::from_uci(input) {
            UciMoveCreationResult::Success(mov) => Move::from_uci(mov, board),
            UciMoveCreationResult::Failure => {
                println!("Invalid move format. Please try again.");
                self.get_move(board)
            }
        }
    }
}

pub struct PlayerAI {

}

impl Player for PlayerAI {
    fn get_move(&self, board: &Board) -> Move {
        todo!()
    }
}