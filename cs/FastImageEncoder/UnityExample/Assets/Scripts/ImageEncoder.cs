using System.Diagnostics;
using System.IO;
using UnityEngine;
using FastImageEncoder;
using UnityEngine.Experimental.Rendering;


[RequireComponent(typeof(Camera))]
public class ImageEncoder : MonoBehaviour
{
    /// <summary>
    /// The render depth buffer.
    /// </summary>
    private const int DEPTH_BUFFER = 24;
    /// <summary>
    /// The texture format.
    /// </summary>
    private const RenderTextureFormat FORMAT = RenderTextureFormat.Default;
    
    
    private Camera cam;
    private ImageBuffers imageBuffers;
    private uint width;
    private uint height;
    private GraphicsFormat graphicsFormat;
    private static bool flipTextures;
    private static readonly Vector2 BlitScale = new Vector2(1, -1);
    private static readonly Vector2 BlitOffset = new Vector2(0, 1);
    
    
    private void Awake()
    {
        // Check if we need to flip the rendered textures.
        flipTextures = SystemInfo.graphicsUVStartsAtTop;
        // Find the camera.
        cam = GetComponent<Camera>();
        // Get the screen size.
        width = (uint)Screen.width;
        height = (uint)Screen.height;
        // Initialize the image buffers to the screen size.
        imageBuffers = new ImageBuffers(width, height);
    }
    
    
    /// <summary>
    /// Rotate around the yaw axis by an angle.
    /// </summary>
    /// <param name="angle">The angle in degrees.</param>
    public void Rotate(float angle = 1)
    {
        transform.Rotate(new Vector3(0, angle, 0));
    }
    
    
    /// <summary>
    /// Write the camera's image to a texture.
    /// </summary>
    public void WriteToTexture()
    {
        // Prepare to render to a RenderTexture.
        RenderTexture renderRT = RenderTexture.GetTemporary(Screen.width, Screen.height, DEPTH_BUFFER, FORMAT, RenderTextureReadWrite.Default);
        RenderTexture prevActiveRT = RenderTexture.active;
        RenderTexture prevCameraRT = cam.targetTexture;
        RenderTexture.active = renderRT;
        cam.targetTexture = renderRT;
        
        // Force the camera to render.
        cam.Render();
        
        // Flip the render texture if needed.
        if (flipTextures)
        {
            RenderTexture flipRT = RenderTexture.GetTemporary(
                Screen.width, Screen.height, DEPTH_BUFFER, FORMAT, RenderTextureReadWrite.Default);
            Graphics.Blit(renderRT, flipRT, BlitScale, BlitOffset);
            Graphics.Blit(flipRT, renderRT);
            RenderTexture.ReleaseTemporary(flipRT);
        }
        
        // Create the texture.
        Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        graphicsFormat = texture.graphicsFormat;

        // Read the pixels.
        texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0, false);
        texture.Apply();
        
        // Copy the texture's raw data to the image encoder.
        texture.GetRawTextureData<byte>().CopyTo(imageBuffers.rawImage);
        
        Destroy(texture);
        
        cam.targetTexture = prevCameraRT;
        RenderTexture.active = prevActiveRT;
        RenderTexture.ReleaseTemporary(renderRT);
    }
    
    
    /// <summary>
    /// Encode the texture and save it as a png file. Returns the time elapsed to encode (not to write to disk).
    ///
    /// You must call WriteToTexture() before calling this.
    ///
    /// Depending on the value of useFastImageEncoder, this will either use Unity's image encoder or FastImageEncoder.
    /// </summary>
    /// <param name="path">The output image file path.</param>
    /// <param name="useFastImageEncoder">If true, use FastImageEncoder. If false, use Unity's image encoding.</param>
    public double EncodeAndWriteToDisk(string path, bool useFastImageEncoder)
    {
        Stopwatch watch = new Stopwatch();
        if (useFastImageEncoder)
        {
            // Encode.
            watch.Start();
            uint length = imageBuffers.Encode();
            watch.Stop();

            // Write the sub-array to disk.
            using (FileStream fs = File.Create(path))
            {
                fs.Write(imageBuffers.encodedImage, 0, (int)length);
            }
        }
        else
        {
            // Encode to png.
            watch.Start();
            byte[] encodedImage = ImageConversion.EncodeArrayToPNG(imageBuffers.rawImage, graphicsFormat, width, height);
            watch.Stop();
            
            // Write to disk.
            File.WriteAllBytes(path, encodedImage);
        }
        return watch.Elapsed.TotalSeconds;
    }
}
