/*! \file */
/*******************************************
 *                                         *
 *  File auto-generated by `::safer_ffi`.  *
 *                                         *
 *  Do not manually edit this file.        *
 *                                         *
 *******************************************/

#pragma warning disable IDE0044, IDE0049, IDE0055, IDE1006,
#pragma warning disable SA1004, SA1008, SA1023, SA1028,
#pragma warning disable SA1121, SA1134,
#pragma warning disable SA1201,
#pragma warning disable SA1300, SA1306, SA1307, SA1310, SA1313,
#pragma warning disable SA1500, SA1505, SA1507,
#pragma warning disable SA1600, SA1601, SA1604, SA1605, SA1611, SA1615, SA1649,

namespace FastImageEncoder {
using System;
using System.Runtime.InteropServices;

public unsafe partial class Ffi {
    private const string RustLib = "fast_image_encoder";
}

/// <summary>
/// Same as [<c>Vec<T></c>][<c>rust::Vec</c>], but with guaranteed <c>#[repr(C)]</c> layout
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 24)]
public unsafe struct Vec_uint8_t {
    public byte * ptr;

    public UIntPtr len;

    public UIntPtr cap;
}

/// <summary>
/// A raw image has a u8 buffer and some image metadata.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 40)]
public unsafe struct RawImage_t {
    /// <summary>
    /// The raw image data.
    /// </summary>
    public Vec_uint8_t buffer;

    /// <summary>
    /// The width of the image.
    /// </summary>
    public UInt32 width;

    /// <summary>
    /// The height of the image.
    /// </summary>
    public UInt32 height;

    /// <summary>
    /// If true, encode to png. If false, encode to jpg.
    /// </summary>
    [MarshalAs(UnmanagedType.U1)]
    public bool png;

    /// <summary>
    /// The image quality (0-100). This is only used for jpgs.
    /// </summary>
    public byte quality;

    /// <summary>
    /// The color type i.e. if this is 1-channel 8bit grayscale, 3-channel 8bit RGB, etc.
    /// </summary>
    public byte color_type;
}

public unsafe partial class Ffi {
    /// <summary>
    /// Encode a single image.
    ///
    /// - <c>raw_image</c> The raw image and its metadata.
    /// - <c>encoded_image</c> The array of the encoded image. It will be filled by this function.
    ///
    /// Returns: The number of pixels used in <c>encoded_image</c> (its true size might be larger).
    /// </summary>
    [DllImport(RustLib, ExactSpelling = true)] public static unsafe extern
    UInt32 encode (
        RawImage_t /*const*/ * raw_image,
        Vec_uint8_t * encoded_image);
}


} /* FastImageEncoder */
