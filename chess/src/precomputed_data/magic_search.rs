// use std::sync::{Arc, Mutex};
// use std::thread::{self, JoinHandle};
// use std::time::Duration;
// use rayon::prelude::*;

// use super::magic_bitboards_data::{get_magic_number};
// use super::square_magic::SquareMagic;

// /// Uses all of the CPU resources to search for the magics
// /// 
// /// Returns vector containing every found magic (or the stock ones if no better magic was found)
// fn parallel_search(
//     is_rook: bool,
//     improvement_timeout: Duration,  // How long to keep searching for improvements per square
// ) -> Vec<SquareMagic> {
//     let results = Arc::new(Mutex::new(Vec::with_capacity(64)));
//     let mut handles: Vec<JoinHandle<()>> = Vec::with_capacity(64);

//     // Create a thread for each square
//     for square in 0..64u8 {
//         let results = Arc::clone(&results);

//         let handle = thread::Builder::new()
//             .name(format!("Thread square {}", square))
//             .stack_size(1 * 1024 * 1024)
//             .spawn(move || {
//                 let mut best_magic = generate_magic_number(square, is_rook);

//                 let mut last_improvement = std::time::Instant::now();

//                 // Generate and test magic numbers until timeout since last improvement
//                 loop {
//                     let magic = generate_magic_number(square, is_rook);
                    
//                     if magic.score.score > best_magic.score.score || 
//                        (magic.score.score == best_magic.score.score && magic.score.actual_size < best_magic.score.actual_size) {
//                         best_magic = magic;
//                         last_improvement = std::time::Instant::now();
                        
//                         // Log progress
//                         println!("Square {}: New best magic found! Score: {:.2}%, Size: {} bytes {:.2} kilobytes", 
//                             square, best_magic.score.score * 100.0, best_magic.score.actual_size, best_magic.score.actual_size as f64 / 1024.0);
//                     }

//                     // Check if we've gone too long without improvement
//                     if last_improvement.elapsed() > improvement_timeout {
//                         break;
//                     }
//                 }

//                 // Store the result
//                 let mut results = results.lock().unwrap();
//                 results.push(best_magic);
//             }).expect(format!("Failed to spawn thread square {}", square).as_str());

//         handles.push(handle);
//     }

//     // Wait for all threads to complete
//     for handle in handles {
//         handle.join().expect("Handles in magic search");
//     }

//     // Sort results by square number for consistent output
//     let mut final_results = results.lock().unwrap().clone();
//     final_results.sort_by_key(|m| m.square);
//     final_results
// }

// /// Searches for the magic for a while, then prints the magics
// pub fn search_magics() {
//     let duration = Duration::from_secs(60);
    
//     // Find magic numbers for rooks (search for 1 minute per square)
//     println!("Finding rook magic numbers...");
//     let rook_magics = parallel_search(
//         true,
//         duration
//     );

//     // Find magic numbers for bishops (search for 1 minute per square)
//     println!("\nFinding bishop magic numbers...");
//     let bishop_magics = parallel_search(
//         false,
//         duration
//     );

//     // Print results
//     println!("\nRook magic numbers:");
//     let mut total_size = 0;
//     for magic in &rook_magics {
//         println!("Square {}: magic = {} 0b{:064b}, score = {:.2}%, size = {} bytes ({} Kb)",
//             magic.square, magic.magic, magic.magic, magic.score.score * 100.0, magic.score.actual_size, magic.score.actual_size / 1024);
//         total_size += magic.score.actual_size;
//     }
//     println!("Rook table total size: {} bytes ({} Kb)", total_size, total_size / 1024);
//     print!("[ ");
//     for magic in &rook_magics {
//         print!("{}, ", magic.magic);
//     }
//     println!(" ]");

//     println!("\nBishop magic numbers:");
//     let mut total_size = 0;
//     for magic in &bishop_magics {
//         println!("Square {}: magic = {} 0b{:064b}, score = {:.2}%, size = {} bytes ({} Kb)",
//             magic.square, magic.magic, magic.magic, magic.score.score * 100.0, magic.score.actual_size, magic.score.actual_size / 1024);
//             total_size += magic.score.actual_size;
//     }
//     println!("Bishop table total size: {} bytes ({} Kb)", total_size, total_size / 1024);
//     print!("[ ");
//     for magic in &bishop_magics {
//         print!("{}, ", magic.magic);
//     }
//     println!(" ]");
// }

// #[cfg(test)]
// mod tests {
//     use super::*;

//     #[test]
//     fn test_find_magic_numbers_parallel() {
//         let duration = Duration::from_millis(100); // Short duration for testing
        
//         // Test rook magics
//         let rook_magics = parallel_search(true, duration);
//         assert_eq!(rook_magics.len(), 64, "Should generate magic numbers for all 64 squares");
        
//         // Test bishop magics
//         let bishop_magics = parallel_search(false, duration);
//         assert_eq!(bishop_magics.len(), 64, "Should generate magic numbers for all 64 squares");
//     }

//     #[test]
//     fn test_square_magic_sorting() {
//         let duration = Duration::from_millis(100);
//         let magics = parallel_search(true, duration);
        
//         // Check if results are sorted by square
//         for i in 0..magics.len() {
//             assert_eq!(magics[i].square, i, "Results should be sorted by square number");
//         }
//     }

//     // #[test]
//     // fn test_magic_number_quality() {
//     //     let duration = Duration::from_millis(100);
//     //     let magics = parallel_search(true, duration);
        
//     //     for magic in magics {
//     //         assert!(magic.score > 0.0, "Score should be positive");
//     //         assert!(magic.score <= 1.0, "Score should not exceed 1.0");
//     //         assert!(magic.magic != 0, "Magic number should not be zero");
//     //         assert!(magic.actual_size > 0, "Actual size should be positive");
//     //     }
//     // }
// }