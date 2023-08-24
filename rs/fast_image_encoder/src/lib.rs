//! Encode raw RGB data into png or jpg data.
//!
//! This crate is designed to be FFI-safe and allocates as little memory as possible.

mod raw_image;
use image::codecs::jpeg::JpegEncoder;
use image::codecs::png::PngEncoder;
use image::{ColorType, ImageEncoder};
pub use raw_image::RawImage;
#[cfg(feature = "headers")]
use safer_ffi::headers::Language::CSharp;
use safer_ffi::prelude::*;
use std::io::Cursor;

/// Encode a single image.
///
/// - `raw_image` The raw image and its metadata.
/// - `encoded_image` The array of the encoded image. It will be filled by this function.
///
/// Returns: The number of pixels used in `encoded_image` (its true size might be larger).
#[ffi_export]
pub fn encode(raw_image: &RawImage, encoded_image: &mut safer_ffi::Vec<u8>) -> u32 {
    // Start a new writer.
    let mut writer = Cursor::new(vec![]);
    // Convert the color type to a format that the image crate can use.
    let color_type = get_color_type(raw_image.color_type);
    // Encode a png image.
    if raw_image.png {
        if let Err(error) = PngEncoder::new(&mut writer).write_image(
            &raw_image.buffer,
            raw_image.width,
            raw_image.height,
            color_type,
        ) {
            panic!("Failed to encode png: {}", error)
        }
    }
    // Encode a jpg image.
    else if let Err(error) = JpegEncoder::new_with_quality(&mut writer, raw_image.quality)
        .write_image(
            &raw_image.buffer,
            raw_image.width,
            raw_image.height,
            color_type,
        )
    {
        panic!("Failed to encode jpg: {}", error)
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
