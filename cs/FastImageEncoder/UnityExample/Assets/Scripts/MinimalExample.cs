using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;


/// <summary>
/// A minimal example of how to encode images from multiple cameras in separate threads using Fast Image Encoder.
/// </summary>
public class MinimalExample : MonoBehaviour
{
    private ImageEncoder[] imageEncoders = new ImageEncoder[10];


    private void Start()
    {
        // Create some cameras.
        GameObject prefab = Resources.Load<GameObject>("ImageEncoder");
        for (int i = 0; i < imageEncoders.Length; i++)
        {
            imageEncoders[i] = Instantiate(prefab).GetComponent<ImageEncoder>();
        }
        // Create the output directory.
        string directory = Path.Combine(Application.dataPath, "../", "Images");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        // Capture images.
        CaptureAll(directory);
        // End.
        EditorApplication.isPlaying = false;
    }
    

    private void CaptureAll(string directory)
    {
        // Render each camera and write to a raw image buffer.
        // This can't be threaded, unfortunately.
        for (int i = 0; i < imageEncoders.Length; i++)
        {
            imageEncoders[i].WriteToTexture();
        }

        // Set up multithreaded image capture.
        Task[] tasks = new Task[imageEncoders.Length];
        // Create a task for each encoding operation.
        for (int i = 0; i < imageEncoders.Length; i++)
        {
            // Get the file path.
            string path = Path.Combine(directory, i + ".png");
            // Create a new index variable to make it thread-safe.
            int index = i;
            // Create and start the task.
            tasks[index] = new Task(() => imageEncoders[index].EncodeAndWriteToDisk(path, true));
            tasks[index].Start();
        }
        // Complete the encoding tasks.
        Task.WaitAll(tasks);
    }
}
