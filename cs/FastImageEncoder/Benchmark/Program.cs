using System.Diagnostics;
using FastImageEncoder;


namespace EncoderTest;

public class Program
{
    private const int NUM_IMAGES = 6;
    private static ImageBuffers[] threadedBuffers = new ImageBuffers[NUM_IMAGES];
    private static UInt32[] lengths = new UInt32[NUM_IMAGES];
    
    
    private static void Main(string[] args)
    {
        string exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        string imagesDirectory = Path.Combine(exePath, "images");
        string outputDirectory = Path.Combine(imagesDirectory, "out");
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }
        
        Console.WriteLine("Sequential encoding:");
        Stopwatch stopWatch = new Stopwatch();
        for (int i = 0; i < NUM_IMAGES; i++)
        {
            string rawImagePath = Path.Combine(imagesDirectory, "img_000" + i + ".bytes");
            byte[] rawImage = File.ReadAllBytes(rawImagePath);
            ImageBuffers buffers = new ImageBuffers(rawImage, 256, 256);
            stopWatch.Start();
            uint length = buffers.Encode();
            stopWatch.Stop();
            Console.WriteLine(stopWatch.Elapsed.TotalSeconds);
            stopWatch.Reset();
            byte[] encodedImage = buffers.GetEncodedImage(length);
            string encodedImagePath = Path.Combine(outputDirectory, "img_000" + i + ".png");
            File.WriteAllBytes(encodedImagePath, encodedImage);
        }
        
        Console.WriteLine("Threaded encoding:");
        for (int i = 0; i < NUM_IMAGES; i++)
        {
            string rawImagePath = Path.Combine(imagesDirectory, "img_000" + i + ".bytes");
            byte[] rawImage = File.ReadAllBytes(rawImagePath);
            threadedBuffers[i] = new ImageBuffers(rawImage, 256, 256);
        }
        Task[] tasks = new Task[NUM_IMAGES];
        for (int i = 0; i < NUM_IMAGES; i++)
        {
            int index = i;
            tasks[i] = new Task(() => lengths[index] = threadedBuffers[index].Encode());
            tasks[i].Start();
        }
        stopWatch.Start();
        Task.WaitAll(tasks);
        stopWatch.Stop();
        Console.WriteLine(stopWatch.Elapsed.TotalSeconds);
    }
}

