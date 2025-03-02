/// Contains the response from a Stockfish command
pub struct StockfishResponse {
    response: String,
}

impl StockfishResponse {
    /// Creates a new Stockfish response from a string
    pub fn new(response: String) -> StockfishResponse {
        StockfishResponse {
            response,
        }
    }

    /// Return the raw response string
    pub fn get_raw_response(&self) -> &str {
        &self.response
    }

    /// Returns a vector with the lines of the response
    pub fn get_lines(&self) -> Vec<&str> {
        self.response.lines().collect()
    }
}