//! Encode raw RGB data into png or jpg data.
//!
//! This crate is designed to be FFI-safe and allocates as little memory as possible.

mod raw_image;
use image::codecs::png::PngEncoder;
use image::{ColorType, ImageEncoder};
use jpeg_encoder::ColorType as JpegColorType;
use jpeg_encoder::Encoder;
pub use raw_image::RawImage;
#[cfg(feature = "headers")]
use safer_ffi::headers::Language::CSharp;
use safer_ffi::prelude::*;
use std::io::Cursor;
use std::ptr;

/// Encode a single image.
///
/// - `raw_image` The raw image and its metadata.
/// - `encoded_image` The array of the encoded image. It will be filled by this function.
/// - `flip_y` If true, flip over y axis.
///
/// Returns: The number of pixels used in `encoded_image` (its true size might be larger).
#[ffi_export]
pub fn encode(raw_image: &mut RawImage, encoded_image: &mut safer_ffi::Vec<u8>, flip_y: bool) -> u32 {
    // Flip the image.
    if flip_y {
        flip(raw_image.width, raw_image.height, &mut raw_image.buffer);
    }
    // Start a new writer.
    let mut writer = Cursor::new(vec![]);
    // Encode a png image.
    if raw_image.png {
        // Convert the color type to a format that the image crate can use.
        let color_type = get_color_type(raw_image.color_type);
        if let Err(error) = PngEncoder::new(&mut writer).write_image(
            &raw_image.buffer,
            raw_image.width,
            raw_image.height,
            color_type,
        ) {
            panic!("Failed to encode png: {}", error)
        }
    } else {
        let color_type = get_jpeg_color_type(raw_image.color_type);
        // Encode a jpg image.
        if let Err(error) = Encoder::new(&mut writer, raw_image.quality).encode(
            &raw_image.buffer,
            raw_image.width as u16,
            raw_image.height as u16,
            color_type,
        ) {
            panic!("Failed to encode jpg: {}", error)
        }
    }
    let w = writer.get_ref();
    let writer_len = w.len();
    encoded_image[0..writer_len].copy_from_slice(w);
    writer_len as u32
}

/// Converts a u8 value to a color type.
fn get_color_type(value: u8) -> ColorType {
    match value {
        0 => ColorType::L8,
        1 => ColorType::La8,
        2 => ColorType::Rgb8,
        3 => ColorType::Rgba8,
        4 => ColorType::L16,
        5 => ColorType::La16,
        6 => ColorType::Rgb16,
        7 => ColorType::Rgba16,
        8 => ColorType::Rgb32F,
        9 => ColorType::Rgba32F,
        _ => unreachable!("Invalid color type value: {}", value),
    }
}

/// Converts a u8 value to a color type.
fn get_jpeg_color_type(value: u8) -> JpegColorType {
    match value {
        0 => JpegColorType::Luma,
        2 => JpegColorType::Rgb,
        3 => JpegColorType::Rgba,
        _ => unreachable!("Invalid color type value: {}", value),
    }
}

fn flip(width: u32, height: u32, data: &mut safer_ffi::Vec<u8>) {
    let width = width as usize * 3;
    let height = height as usize;
    for (r0, r1) in (0..height).zip((0..height).rev()) {
        let x0 = r0 * width;
        let x1 = r1 * width;
        let ptr = data.as_mut_ptr();
        unsafe {
            ptr::swap_nonoverlapping(ptr.add(x0), ptr.add(x1), width);
        }
    }
}

#[cfg(feature = "headers")]
pub fn generate_headers() -> ::std::io::Result<()> {
    let builder = safer_ffi::headers::builder().with_language(CSharp);
    if ::std::env::var("HEADERS_TO_STDOUT")
        .ok()
        .map_or(false, |it| it == "1")
    {
        builder.to_writer(::std::io::stdout()).generate()?
    } else {
        builder
            .to_file(&format!(
                "../../cs/FastImageEncoder/FastImageEncoder/NativeBindings.cs"
            ))?
            .generate()?
    }
    Ok(())
}
