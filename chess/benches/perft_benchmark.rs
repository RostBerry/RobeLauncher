use std::time::Duration;

use chess::{board::Board, config};
use criterion::{criterion_group, criterion_main, BenchmarkId, Criterion};

fn benchmark_comparison (c: &mut Criterion) {
    let mut group = c.benchmark_group("robe_perft");
    
    // Shared resources
    let mut board = Board::from_fen(core::config::CHESS_BENCHMARK_FEN);

    group.bench_function(BenchmarkId::new("perft_benchmark_depth_4", ""), |b| {
        b.iter(|| {
            criterion::black_box(chess::perft::run_perft(config::PERFT_DEPTH, &mut board));
        });
    });

    group.finish();
}
criterion_group! {
    name = benches;
    config = Criterion::default().sample_size(230).measurement_time(Duration::from_secs(10));
    targets = benchmark_comparison
}

criterion_main!(benches);