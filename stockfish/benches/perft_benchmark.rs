use std::time::Duration;

use criterion::{criterion_group, criterion_main, Criterion, BenchmarkId};
use stockfish::stockfish_service;

fn benchmark_comparison(c: &mut Criterion) {
    let mut group = c.benchmark_group("stockfish_perft");
    let rt = tokio::runtime::Runtime::new().unwrap();

    rt.block_on(stockfish_service::init_global_service(true)).unwrap();
    rt.block_on(stockfish_service::set_position("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8")).unwrap();

    group.bench_function(BenchmarkId::new("perft_benchmark", ""), |b| {
        b.iter(|| {
            rt.block_on(async {
                match stockfish_service::go_perft(4).await {
                    Ok(response) => criterion::black_box(response.get_node_count()),
                    Err(_) => panic!("Error running perft"),
                }
            })
        });
    });

    group.finish();
}

criterion_group! {
    name = benches;
    config = Criterion::default().sample_size(1000).measurement_time(Duration::from_secs(10));
    targets = benchmark_comparison
}
criterion_main!(benches);