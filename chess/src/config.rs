use std::path::Path;

pub const DO_ALL_PROMOTIONS: bool = false;
pub fn get_magic_json_path() -> &'static str {
    if Path::new("chess/src/precomputed_data/magics.json").exists() {
        "chess/src/precomputed_data/magics.json"
    } else {
        "src/precomputed_data/magics.json"
    }
}

pub const PERFT_DEPTH: u8 = 4;