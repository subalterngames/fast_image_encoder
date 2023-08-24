namespace FastImageEncoder;

/// <summary>
/// A safe wrapper for encoding raw RGB data.
/// </summary>
public struct ImageBuffers
{
    /// <summary>
    /// A cached array for the raw image.
    /// </summary>
    public byte[] rawImage;
    /// <summary>
    /// A cached array for the encoded image. This array's length is the same as rawImage so that the encoded image can always fit in it. The Encode() method returns the size of the encoded sub-array.
    /// </summary>
    public byte[] encodedImage;
    /// <summary>
    /// The width of the image.
    /// </summary>
    private UInt32 width;
    /// <summary>
    /// The height of the image.
    /// </summary>
    private UInt32 height;
    /// <summary>
    /// The length of each array.
    /// </summary>
    private UInt32 length;
    /// <summary>
    /// The color type.
    /// </summary>
    private byte colorType;


    public ImageBuffers(UInt32 width, UInt32 height, ColorType colorType = ColorType.Rgb8)
    {
        this.width = width;
        this.height = height;
        this.colorType = (byte)colorType;
        // Get the image depth.
        UInt32 depth;
        if (colorType == ColorType.L8)
        {
            depth = 1;
        }
        else if (colorType == ColorType.La8 || colorType == ColorType.L16)
        {
            depth = 2;
        }
        else if (colorType == ColorType.Rgb8)
        {
            depth = 3;
        }
        else if (colorType == ColorType.Rgba8 || colorType == ColorType.La16)
        {
            depth = 4;
        }
        else if (colorType == ColorType.Rgb16)
        {
            depth = 6;
        }
        else if (colorType == ColorType.Rgba16)
        {
            depth = 8;
        }
        else if (colorType == ColorType.Rgb32F)
        {
            depth = 12;
        }
        else if (colorType == ColorType.Rgba32F)
        {
            depth = 16;
        }
        else
        {
            throw new Exception("Undefined color type: " + colorType);
        }
        // Get the size of the array.
        length = width * height * depth;
        // Cache the arrays.
        rawImage = new byte[length];
        encodedImage = new byte[length];
    }
    
    
    public ImageBuffers(byte[] rawImage, UInt32 width, UInt32 height, ColorType colorType = ColorType.Rgb8)
    {
        this.width = width;
        this.height = height;
        this.rawImage = rawImage;
        length = (UInt32)rawImage.Length;
        encodedImage = new byte[rawImage.Length];
        this.colorType = (byte)colorType;
    }
    
    
    /// <summary>
    /// Encode this image. Returns the length of the sub-array in encodedImage that contains the image.
    /// </summary>
    /// <param name="png">If true, encode to png. If false, encode to jpg.</param>
    /// <param name="quality">The jpg quality (0-100).</param>
    public UInt32 Encode(bool png = true, byte quality = 75)
    {
        unsafe
        {
            fixed (byte* rawImagePointer = rawImage, encodedImagePointer = encodedImage)
            {
                // Convert the raw image to a vec.
                Vec_uint8_t rawImageVec = new Vec_uint8_t()
                {
                    ptr = rawImagePointer,
                    len = length,
                    cap = length
                };
                // Prepare the raw image data.
                RawImage_t rawImageT = new RawImage_t()
                {
                    buffer = rawImageVec,
                    width = width,
                    height = height,
                    color_type = colorType,
                    png = png,
                    quality = quality
                };
                // Get the encoded image vec.
                Vec_uint8_t encodedImageVec = new Vec_uint8_t()
                {
                    ptr = encodedImagePointer,
                    len = length,
                    cap = length
                };
                // Encode.
                return Ffi.encode(&rawImageT, &encodedImageVec);
            }
        }
    }
    
    
    /// <summary>
    /// Returns the subarray of encodedImage that contains the encoded image.
    /// </summary>
    /// <param name="length">The expected length of the encoded image.</param>
    public byte[] GetEncodedImage(UInt32 length)
    {
        byte[] image = new byte[length];
        Buffer.BlockCopy(encodedImage, 0, image, 0, (int)length);
        return image;
    }
}