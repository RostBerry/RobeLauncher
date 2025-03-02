use std::time::Duration;

use chess::{board::Board, move_generation::{attack_calculator::AttackCalculator, move_gen}};
use criterion::{criterion_group, criterion_main, Criterion, BenchmarkId};

fn benchmark_comparison(c: &mut Criterion) {
    let mut group = c.benchmark_group("robe_move_generation");
    
    // Shared resources
    let mut board = Board::from_fen(core::config::CHESS_BENCHMARK_FEN);
    let mut legal_moves = Vec::with_capacity(218);

    group.bench_function(BenchmarkId::new("legal_movegen_benchmark", ""), |b| {
        b.iter(|| {
            criterion::black_box(move_gen::generate_moves(&mut legal_moves, &mut board));
            legal_moves.clear();
        });
    });

    group.bench_function(BenchmarkId::new("att_calc_benchmark", ""), |b| {
        b.iter(|| {
            criterion::black_box(AttackCalculator::new(&board));
        });
    });

    group.finish();
}

criterion_group! {
    name = benches;
    config = Criterion::default().sample_size(1000).measurement_time(Duration::from_secs(5));
    targets = benchmark_comparison
}
criterion_main!(benches);