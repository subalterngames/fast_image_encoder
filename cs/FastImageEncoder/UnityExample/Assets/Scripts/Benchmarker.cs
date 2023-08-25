using System.IO;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;


public class Benchmarker : MonoBehaviour
{
    private const int NUM_ROTATIONS = 360;
    private const int NUM_CAMERAS = 10;

    private ImageEncoder[] imageEncoders = new ImageEncoder[NUM_CAMERAS];
    private double[] encodingTimes = new double[NUM_CAMERAS];
    private string imagesDirectory;


    private void Awake()
    { 
        imagesDirectory = Path.Combine(Application.dataPath, "../", "Images");
        if (!Directory.Exists(imagesDirectory))
        {
            Directory.CreateDirectory(imagesDirectory);
        }
        if (Screen.width != 256 || Screen.height != 256)
        {
            Debug.LogWarning("Your screen is not 256x256, which is what was used to benchmark FastImageEncoder");
        }
        PopulateScene();
        CreateImageEncoders();
        StartCoroutine(RunAll());
    }
    
    
    /// <summary>
    /// Populate the scene with cubes.
    /// </summary>
    private void PopulateScene()
    {
        float angle = 0;
        while (angle < 360)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            float a = Mathf.Deg2Rad * angle;
            cube.transform.position = new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)) * 1.5f;
            angle += 30;
        }
    }
    
    
    /// <summary>
    /// Create some ImageEncoders from a prefab.
    /// </summary>
    private void CreateImageEncoders()
    {
        GameObject prefab = Resources.Load<GameObject>("ImageEncoder");
        for (int i = 0; i < NUM_CAMERAS; i++)
        {
            imageEncoders[i] = Instantiate(prefab).GetComponent<ImageEncoder>();
        }
    }


    private IEnumerator Run(bool threaded, bool useFastImageEncoder)
    {
        double allEncodedTimes = 0;
        double eachEncodedTimes = 0;
        for (int i = 0; i < NUM_ROTATIONS; i++)
        {
            // Wait for the end of the frame to render.
            yield return new WaitForEndOfFrame();
            
            // Rotate the cameras and capture the image.
            for (int j = 0; j < imageEncoders.Length; j++)
            {
                imageEncoders[j].Rotate();
                imageEncoders[j].WriteToTexture();

            }

            // Encode with threading.
            if (threaded)
            {
                Task[] tasks = new Task[NUM_CAMERAS];
                for (int j = 0; j < NUM_CAMERAS; j++)
                {
                    int index = j;
                    string path = Path.Combine(imagesDirectory, i + "_" + j + ".png");
                    tasks[j] = new Task(() => encodingTimes[index] = imageEncoders[index].EncodeAndWriteToDisk(path, useFastImageEncoder));
                    tasks[j].Start();
                }
                // Encode everything in a thread.
                Stopwatch all = new Stopwatch();
                all.Start();
                Task.WaitAll(tasks);
                all.Stop();
                for (int j = 0; j < NUM_CAMERAS; j++)
                {
                    // Append the time to encode each image.
                    eachEncodedTimes += encodingTimes[j];
                }
                allEncodedTimes += all.Elapsed.TotalSeconds;
            }
            // Encode sequentially.
            else
            {
                Stopwatch all = new Stopwatch();
                all.Start();
                for (int j = 0; j < imageEncoders.Length; j++)
                {
                    all.Stop();;
                    imageEncoders[j].WriteToTexture();
                    all.Start();
                    string path = Path.Combine(imagesDirectory, i + "_" + j + ".png");
                    eachEncodedTimes += imageEncoders[j].EncodeAndWriteToDisk(path, useFastImageEncoder);
                    all.Stop();
                }
                allEncodedTimes += all.Elapsed.TotalSeconds;
            }
        }

        Debug.Log(
            "Threaded: " + threaded + "\nUse Fast ImageEncoder: " + useFastImageEncoder +
            "\nAvg. elapsed time for all cameras per frame: " + allEncodedTimes / NUM_ROTATIONS +
            " \nAvg. elapsed time for each camera per frame: " +
            eachEncodedTimes / (NUM_ROTATIONS * imageEncoders.Length));
    }


    private IEnumerator RunAll()
    {
        //yield return Run(false, false);
        //yield return Run(true, false);
        yield return Run(false, true);
        yield return Run(true, true);
        EditorApplication.isPlaying = false;
    }
}