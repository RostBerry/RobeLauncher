use chess::r#move::{UciMoveCreationResult, UciMove};
use chess::perft::perft_node::PerftNode;

use crate::stockfish_wrapper::stockfish_response::StockfishResponse;


/// Contains the response from the Stockfish perft command
pub struct StockfishPerftResponse {
    nodes: Vec<PerftNode>,
    info_lines: Vec<String>,
}

impl StockfishPerftResponse {
    /// Parses a Stockfish response into a Perft response
    pub fn from_response(response: StockfishResponse) -> StockfishPerftResponse {
        let mut nodes = Vec::new();
        let mut info_lines = Vec::new();

        for line in response.get_lines() {
            if line.starts_with("info string") {
                info_lines.push(line.to_string());
            } else {
                let parts: Vec<&str> = line.split(": ").collect();
                if parts.len() == 2 {
                    let uci_mov = UciMove::from_uci(parts[0]);
                    if let UciMoveCreationResult::Success(mov) = uci_mov {
                        if let Ok(node_count) = parts[1].parse::<u64>() {
                            nodes.push(PerftNode::new(mov, node_count as usize));
                        }
                    }
                }
            }
        }

        StockfishPerftResponse {
            nodes,
            info_lines,
        }
    }

    /// Returns the total number of nodes
    pub fn get_node_count(&self) -> usize {
        self.nodes.len()
    }

    /// Returns the number of the nodes from each move
    pub fn get_nodes(&self) -> &Vec<PerftNode> {
        &self.nodes
    }

    /// Returns a vector with info lines from the Perft response
    pub fn get_info_lines(&self) -> &Vec<String> {
        &self.info_lines
    }
}