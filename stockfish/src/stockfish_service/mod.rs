pub mod stockfish_perft_response;

use once_cell::sync::Lazy;
use stockfish_perft_response::StockfishPerftResponse;
use tokio::sync::Mutex;
use std::{path::Path, sync::Arc};

use crate::{config, stockfish_wrapper::{stockfish_response::StockfishResponse, StockfishWrapper}};

// Global static service wrapped in Arc and Mutex for thread-safe access
static STOCKFISH_SERVICE: Lazy<Arc<Mutex<Option<StockfishWrapper>>>> = Lazy::new(|| {
    Arc::new(Mutex::new(None))
});

// Initialization function to set up the service globally
pub async fn init_global_service(is_benchmark: bool) -> std::io::Result<()> {
    let full_path = if is_benchmark {"."} else {""}.to_string() + config::STOCKFISH_PATH;
    let stockfish_path = Path::new(&full_path);
    if !stockfish_path.exists() {
        return Err(std::io::Error::new(
            std::io::ErrorKind::NotFound,
            format!("Stockfish executable not found at: {}", stockfish_path.display())
        ));
    }
    let service = StockfishWrapper::new(
        stockfish_path.to_str().ok_or_else(|| std::io::Error::new(
            std::io::ErrorKind::InvalidData,
            "Invalid path to Stockfish executable"
        ))?
    ).await?;

    //Reading the welcome message
    match service.read_output("Stockfish 17 by the Stockfish developers (see AUTHORS file)").await {
        Ok(response) => {
            for line in response.get_lines() {
                println!("Stockfish welcome line: {}", line);
            }
        },
        Err(e) => {
            panic!("Error reading Stockfish welcome message: {}", e);
        }
    }
    // Initializing Stockfish process
    match service.process_command("uci", "uciok").await {
        Ok(response) => {
            if response.get_lines().contains(&"uciok")  {
                for line in response.get_lines() {
                    println!("Stockfish init line: {}", line);
                }
                println!("Stockfish initialized");
            } else {
                panic!("Stockfish not initialized: {}", response.get_raw_response());
            }
        },
        Err(e) => {
            panic!("Error initializing Stockfish: {}", e);
        }
    }
    match service.process_command("isready","readyok").await {
        Ok(response) => {
            if response.get_raw_response().trim() == "readyok" {
                println!("Stockfish ready");
            } else {
                panic!("Unexpected response from Stockfish: {}", response.get_raw_response());
            }
        },
        Err(e) => {
            panic!("Error checking Stockfish readiness: {}", e);
        }
    }
    
    let mut service_lock = STOCKFISH_SERVICE.lock().await;
    *service_lock = Some(service);
    Ok(())
}

pub async fn go_perft(depth: u8) -> std::io::Result<StockfishPerftResponse> {
    let command = format!("go perft {}", depth);
    let response = process_command(&command, "Nodes searched:").await?;
    let perft_response = StockfishPerftResponse::from_response(response);
    Ok(perft_response)

}

pub async fn set_position(fen: &str) -> std::io::Result<()> {
    let command = format!("position fen {}", fen);
    send_command(&command).await?;
    Ok(())
}

async fn send_command(command: &str) -> std::io::Result<()> {
    let service_mutex = STOCKFISH_SERVICE.lock().await;
    if let Some(service) = service_mutex.as_ref() {
        service.send_command(command).await?;
        Ok(())
    } else {
        Err(std::io::Error::new(
            std::io::ErrorKind::Other, 
            "Stockfish service not initialized"
        ))
    }
}

async fn process_command(command: &str, stop_word: &str) -> std::io::Result<StockfishResponse> {
    let service_mutex = STOCKFISH_SERVICE.lock().await;
    if let Some(service) = service_mutex.as_ref() {
        let response = service.process_command(command, stop_word).await?;
        Ok(response)
    } else {
        Err(std::io::Error::new(
            std::io::ErrorKind::Other, 
            "Stockfish service not initialized"
        ))
    }
}
