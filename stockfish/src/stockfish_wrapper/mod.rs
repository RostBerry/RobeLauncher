pub mod stockfish_response;

use std::process::{Child, Command, Stdio};
use std::io::{BufRead, BufReader, Write};
use stockfish_response::StockfishResponse;
use tokio::sync::Mutex;
use tokio::time::{timeout, Duration};

/// Wrapper for the Stockfish engine process
pub struct StockfishWrapper {
    process: Mutex<Child>,
}

impl StockfishWrapper {
    pub async fn new(path: &str) -> std::io::Result<Self> {
        let process = Command::new(path)
            .stdin(Stdio::piped())
            .stdout(Stdio::piped())
            .stderr(Stdio::piped())
            .spawn()?;

        Ok(Self { 
            process: Mutex::new(process) 
        })
    }

    /// Sends a command to the Stockfish engine
    pub async fn send_command(&self, command: &str) -> std::io::Result<()> {
        let mut process = self.process.lock().await;

        if let Some(stdin) = process.stdin.as_mut() {
            stdin.write_all(command.as_bytes())?;
            stdin.write_all(b"\n")?;
            stdin.flush()?;
        }

        Ok(())
    }

    /// Reads the entire output from the Stockfish engine
    pub async fn read_output(&self, stop_word: &str) -> std::io::Result<StockfishResponse> {
        let mut process = self.process.lock().await;
        // Read response with timeout
        if let Some(stdout) = process.stdout.as_mut() {
            let mut reader = BufReader::new(stdout);
            
            // Use timeout to prevent indefinite waiting
            let response_result = timeout(
                Duration::from_secs(10), 
                async {
                    let mut response = String::new();
                    loop {
                        let mut line = String::new();
                        match reader.read_line(&mut line) {
                            Ok(0) => continue,
                            Ok(_) => {
                                response.push_str(&line);
                                if line.contains(stop_word) {
                                    break;
                                }
                            },
                            Err(e) => panic!("Error reading Stockfish response: {}", e),
                        }
                    }
                    Ok(response)
                }
            ).await;

            match response_result {
                Ok(Ok(response)) => Ok(StockfishResponse::new(response)),
                Ok(Err(e)) => Err(e),
                Err(_) => Err(std::io::Error::new(
                    std::io::ErrorKind::TimedOut, 
                    "Command execution timed out"
                ))
            }
        } else {
            Err(std::io::Error::new(
                std::io::ErrorKind::Other, 
                "Could not access stdout"
            ))
        }
    }

    /// Sends a command to the Stockfish engine and returns the response
    pub async fn process_command(&self, command: &str, stop_word: &str) -> std::io::Result<StockfishResponse> {
        self.send_command(command).await?;
        match self.read_output(stop_word).await {
            Ok(response) => Ok(response),
            Err(e) => Err(e),
        }
    }
}