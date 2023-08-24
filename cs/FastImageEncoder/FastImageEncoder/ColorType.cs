namespace FastImageEncoder;


/// <summary>
/// An enumeration over supported color types and bit depths.
/// </summary>
public enum ColorType : byte
{
    L8 = 0,
    La8 = 1,
    Rgb8 = 2,
    Rgba8 = 3,
    L16 = 4,
    La16 = 5,
    Rgb16 = 6,
    Rgba16 = 7,
    Rgb32F = 8,
    Rgba32F = 9, 
}