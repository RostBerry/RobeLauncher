use chess::{board::Board, board_representation, config, move_generation::{attack_calculator::AttackCalculator, move_gen}, perft};

#[global_allocator]
static GLOBAL: mimalloc::MiMalloc = mimalloc::MiMalloc;

#[tokio::main]
async fn main() -> std::io::Result<()> {
    match stockfish::stockfish_service::init_global_service(false).await {
        Ok(_) => println!("Stockfish service initialized"),
        Err(e) => panic!("Error initializing Stockfish service: {}", e)
    }

    let mut board = Board::from_fen("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -");
    let attack_calc = AttackCalculator::new(&board);
    let mut legal_moves = Vec::with_capacity(218);
    move_gen::generate_moves(&mut legal_moves, &mut board);
    board_representation::print_board(&board);
    AttackCalculator::print(&attack_calc);
    move_gen::print(&legal_moves);

    perft::run_perft(config::PERFT_DEPTH, &mut board);

    return Ok(());
}