#[cfg(feature = "headers")]
fn main() -> ::std::io::Result<()> {
    ::fast_image_encoder::generate_headers()
}

#[cfg(not(feature = "headers"))]
fn main() {}
