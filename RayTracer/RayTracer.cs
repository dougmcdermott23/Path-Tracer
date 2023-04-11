using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;

namespace RayTracer;

using Shapes;
using System.Drawing.Text;
using System.Net.WebSockets;
using static Constants;
using static Utils.VectorUtils;

public class RayTracer
{
    private Camera Camera { get; }

    private ColorBuffer ColorBuffer { get; }

    private ShapeCollection World { get; }

    private int MaxDepth { get; }

    private int SamplesPerPixel { get; }

    private Random Random { get; } = new();

    private int ConcurrencyLimit { get; }

    private Semaphore Semaphore { get; }

    private WaitHandle[] WaitHandles { get; }

    private SynchronizedCollection<Task> TaskList { get; } = new();

    private CancellationTokenSource CancellationTokenSource { get; } = new();

    private int PixelsProcessed = 0;

    public RayTracer(Camera camera,
                     ColorBuffer colorBuffer,
                     ShapeCollection world,
                     int maxDepth,
                     int samplesPerPixel,
                     int concurrencyLimit)
    {
        Camera           = camera;
        ColorBuffer      = colorBuffer;
        World            = world;
        MaxDepth         = maxDepth;
        SamplesPerPixel  = samplesPerPixel;
        ConcurrencyLimit = concurrencyLimit;

        Semaphore   = new(ConcurrencyLimit, ConcurrencyLimit);
        WaitHandles = new WaitHandle[]
                      {
                          CancellationTokenSource.Token.WaitHandle,
                          Semaphore
                      };
    }

    /// <summary>
    ///
    ///     Run the ray tracer with the defined properties. This process is multithreaded to optimize runtime.
    ///
    /// </summary>
    ///
    /// <exception cref="TaskCanceledException">Ray tracer was cancelled before all pixels were processed</exception>
    public void Run()
    {
        foreach (var coord in EnumerateImagePlane())
        {
            var startTask   = new ManualResetEvent(false);
            var taskStarted = new ManualResetEvent(false);

            WaitHandle.WaitAny(WaitHandles);

            if (!CancellationTokenSource.IsCancellationRequested)
            {
                var task = Task.Run(() =>
                                    {
                                        startTask.WaitOne();
                                        taskStarted.Set();

                                        try
                                        {
                                            ProcessPixel(coord.X, coord.Y);
                                            LogProgress();
                                        }
                                        catch (Exception e)
                                        {
                                            CancellationTokenSource.Cancel();
                                            taskStarted.Set();

                                            Console.WriteLine(e);
                                        }
                                        finally
                                        {
                                            Semaphore.Release();
                                            taskStarted.Set();
                                        }
                                    },
                                    CancellationTokenSource.Token);

                task.ContinueWith(t => TaskList.Remove(t));

                TaskList.Add(task);
                startTask.Set();
                taskStarted.WaitOne();
            }
            else
            {
                throw new TaskCanceledException("All tasks are cancelled due to a cancellation token request.");
            }
        }

        WaitForAllTasks();

        var bitmap = CreateBitmap();

        bitmap.Save("raytracer.jpg");

        #region Local Methods

        IEnumerable<(int X, int Y)> EnumerateImagePlane()
        {
            for (var y = 0; y < ColorBuffer.Height; y++)
            {
                for (var x = 0; x < ColorBuffer.Width; x++)
                {
                    yield return (x, y);
                }
            }
        }

        void WaitForAllTasks()
        {
            try
            {
                Task.WaitAll(TaskList.ToList().Where(t => t is not null).ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
    }

    private void ProcessPixel(int x, int y)
    {
        var pixelColor = new Vector3();

        for (var sample = 0; sample < SamplesPerPixel; sample++)
        {
            var u = (float)(x + Random.NextDouble()) / ColorBuffer.Width;
            var v = (float)(y + Random.NextDouble()) / ColorBuffer.Height;

            var ray = Camera.GetRay(u, v);

            var rayColor = Vector3.One;
            var incomingLight = Vector3.Zero;

            TraceRay(ray,
                     World,
                     MaxDepth,
                     ref rayColor,
                     ref incomingLight);

            pixelColor += incomingLight;
        }

        pixelColor /= SamplesPerPixel;

        pixelColor = Vector3.Clamp(pixelColor, Vector3.Zero, Vector3.One);

        ColorBuffer.WriteColorToBuffer(pixelColor, x, y);
    }

    private void TraceRay(Ray ray,
                          ShapeCollection shapeCollection,
                          int currentDepth,
                          ref Vector3 rayColor,
                          ref Vector3 incomingLight)
    {
        if (currentDepth <= 0)
        {
            return;
        }

        if (!shapeCollection.Hit(ray, MinimumRoot, MaximumRoot, out var hitRecord))
        {
            return;
        }

        if (hitRecord is null)
        {
            throw new ArgumentException("Hit record shall not be null if there is a hit detected.");
        }

        var diffuseTarget  = hitRecord.Value.Normal + CosineWeightedDistribution(Random, hitRecord.Value.Normal);
        var specularTarget = Vector3.Reflect(ray.Direction, hitRecord.Value.Normal);

        var target = Vector3.Lerp(diffuseTarget, specularTarget, hitRecord.Value.Material.Smoothness);

        var emittedLight = hitRecord.Value.Material.EmissionColor * hitRecord.Value.Material.EmissionStrength;

        incomingLight += emittedLight * rayColor;
        rayColor      *= hitRecord.Value.Material.MaterialColor;

        TraceRay(new Ray(hitRecord.Value.Point, target),
                 shapeCollection,
                 currentDepth - 1,
                 ref rayColor,
                 ref incomingLight);
    }

    private void LogProgress()
    {
        Interlocked.Increment(ref PixelsProcessed);

        if (PixelsProcessed % ColorBuffer.Width != 0)
        {
            return;
        }

        var scanlinesProcessed = PixelsProcessed / ColorBuffer.Width;

        Console.WriteLine($"Processed Scanlines: {scanlinesProcessed} of {ColorBuffer.Height}");
    }

    private Bitmap CreateBitmap()
    {
        var bitmap = new Bitmap(ColorBuffer.Width, ColorBuffer.Height, PixelFormat.Format32bppRgb);

        for (var y = 0; y < ColorBuffer.Height; y++)
        {
            for (var x = 0; x < ColorBuffer.Width; x++)
            {
                var color = ColorBuffer.ReadColorFromBuffer(x, y);

                // Camera viewport uses uv coordiantes where 0,0 is bottom left
                // The y coordinate must be inversed as 0,0 is the top left for Bitmap
                bitmap.SetPixel(x, ColorBuffer.Height - 1 - y, color);
            }
        }

        return bitmap;
    }
}
