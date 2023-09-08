use safer_ffi::prelude::derive_ReprC;

/// A raw image has a u8 buffer and some image metadata.
#[derive_ReprC]
#[repr(C)]
pub struct RawImage {
    /// The raw image data.
    pub buffer: safer_ffi::Vec<u8>,
    /// The width of the image.
    pub width: u32,
    /// The height of the image.
    pub height: u32,
    /// If true, encode to png. If false, encode to jpg.
    pub png: bool,
    /// The image quality (0-100). This is only used for jpgs.
    pub quality: u8,
    /// The color type i.e. if this is 1-channel 8bit grayscale, 3-channel 8bit RGB, etc.
    pub color_type: u8,
}