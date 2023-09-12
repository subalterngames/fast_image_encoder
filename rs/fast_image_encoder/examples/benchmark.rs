use fast_image_encoder::{encode, RawImage};
use std::fs::{create_dir_all, read_dir, remove_dir_all, File};
use std::io::{Read, Write};
use std::path::Path;
use std::time::Instant;

fn main() {
    // Load images into a vec of vecs.
    let paths = read_dir("images");
    assert!(paths.is_ok(), "Couldn't read images");
    let paths = paths.unwrap();
    let out_directory = Path::new("images/out");
    if out_directory.exists() {
        remove_dir_all(out_directory).unwrap();
    }
    create_dir_all(out_directory).unwrap();
    let mut i = 0u32;
    let mut t = 0.0;
    for path in paths {
        assert!(path.is_ok(), "Path is not ok: {:?}", path);
        let path = path.unwrap().path();
        if path.is_dir() {
            continue;
        }
        let f = File::open(&path);
        assert!(f.is_ok(), "Error loading file: {:?}", &path);
        let mut f = f.unwrap();
        let mut buffer = vec![];
        f.read_to_end(&mut buffer).unwrap();
        let mut encoded_image: safer_ffi::Vec<u8> = safer_ffi::Vec::from(vec![0u8; 256 * 256 * 3]);
        let mut raw_image = RawImage {
            buffer: safer_ffi::Vec::from(buffer),
            width: 256,
            height: 256,
            png: true,
            quality: 0,
            color_type: 2,
        };
        let now = Instant::now();
        encode(&mut raw_image, &mut encoded_image);
        let dt = now.elapsed().as_micros() as f64 / 1000000.0;
        println!("{}", dt);
        t += dt;
        let mut f = File::create(out_directory.join(format!("{}.png", i))).unwrap();
        f.write(&encoded_image).unwrap();
        i += 1;
    }
    println!("Average: {}", t / i as f64);
}
