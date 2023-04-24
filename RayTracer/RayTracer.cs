using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;

namespace RayTracer;

using Shapes;

using static Constants;
using static Utils.MathDM;
using static Utils.VectorUtils;

public class RayTracer
{
    private Camera Camera { get; }

    private ColorBuffer ColorBuffer { get; }

    private ShapeCollection World { get; }

    private int MaxDepth { get; }

    private int SamplesPerPixel { get; }

    private int ConcurrencyLimit { get; }

    private bool UseSkybox { get; }

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
                     int concurrencyLimit,
                     bool useSkybox = false)
    {
        Camera           = camera;
        ColorBuffer      = colorBuffer;
        World            = world;
        MaxDepth         = maxDepth;
        SamplesPerPixel  = samplesPerPixel;
        ConcurrencyLimit = concurrencyLimit;
        UseSkybox        = useSkybox;

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

        var state = (uint)(y * ColorBuffer.Width + x);

        for (var sample = 0; sample < SamplesPerPixel; sample++)
        {
            var u = (float)(x + RandomValue(ref state)) / ColorBuffer.Width;
            var v = (float)(y + RandomValue(ref state)) / ColorBuffer.Height;

            var ray                 = Camera.GetRay(u, v, ref state);
            var initialContribution = 1.0f;
            var rayColor            = Vector3.One;
            var incomingLight       = Vector3.Zero;

            TraceRay(ray,
                     World,
                     MaxDepth,
                     initialContribution,
                     rayColor,
                     ref incomingLight,
                     ref state);

            pixelColor += incomingLight;
        }

        pixelColor /= SamplesPerPixel;

        pixelColor = Vector3.Clamp(pixelColor, Vector3.Zero, Vector3.One);

        ColorBuffer.WriteColorToBuffer(pixelColor, x, y);
    }

    private void TraceRay(Ray ray,
                          ShapeCollection shapeCollection,
                          int currentDepth,
                          float proportionalContribution,
                          Vector3 rayColor,
                          ref Vector3 incomingLight,
                          ref uint state,
                          bool applyBeers = false)
    {
        if (currentDepth <= 0)
        {
            return;
        }

        if (!shapeCollection.TryGetFirstHit(ray,
                                            MinimumRoot,
                                            MaximumRoot,
                                            out var hitRecord))
        {
            Skybox(ref rayColor,
                   ref incomingLight);

            return;
        }

        Shade(ref rayColor,
              ref incomingLight);

        ComputeReflected(rayColor,
                         ref incomingLight,
                         ref state);

        ComputeTransmitted(rayColor,
                           ref incomingLight,
                           ref state);

        #region LocalMethod

        void Skybox(ref Vector3 rayColor,
                    ref Vector3 incomingLight)
        {
            if (!UseSkybox)
            {
                return;
            }

            var t = 0.5f * (ray.Direction.Y + 1.0f);

            var skyColor = Vector3.Lerp(DefaultSkyboxMin,
                                        DefaultSkyboxMax,
                                        t);

            incomingLight += skyColor * rayColor * proportionalContribution;
        }

        void Shade(ref Vector3 rayColor,
                   ref Vector3 incomingLight)
        {
            var emittedLight = hitRecord.Material.EmissionColor * hitRecord.Material.EmissionStrength;

            // Beer's Law for light absorbtion through a material
            // Source: https://en.wikipedia.org/wiki/Beer%E2%80%93Lambert_law
            if (applyBeers)
            {
                var absorbance = -1.0f * (float)hitRecord.Root * hitRecord.Material.Absorbance * hitRecord.Material.AbsorbanceColor;

                rayColor *= new Vector3(MathF.Exp(absorbance.X),
                                        MathF.Exp(absorbance.Y),
                                        MathF.Exp(absorbance.Z));
            }

            incomingLight += emittedLight * rayColor * proportionalContribution;

            // We are not applying Beer's law, update the ray color using the color of the reflected shapes surface
            if (!applyBeers)
            {
                rayColor *= hitRecord.Material.MaterialColor;
            }
        }

        void ComputeReflected(Vector3 rayColor,
                              ref Vector3 incomingLight,
                              ref uint state)
        {
            if (hitRecord.Material.ReflectiveConstant == 0.0f)
            {
                return;
            }

            // This prevents reflections inside of objects.
            // Reflections in objects should be handled by Fresnel in the transmissive ray.
            if (!hitRecord.FrontFace)
            {
                return;
            }

            var diffuseTarget = hitRecord.Normal + CosineWeightedDistribution(ref state, hitRecord.Normal);

            var specularPerturbation = hitRecord.Material.Fuzziness > 0
                                           ? hitRecord.Material.Fuzziness * RandomInUnitSphere(ref state)
                                           : Vector3.Zero;
            var specularTarget       = Vector3.Reflect(ray.Direction, hitRecord.Normal);

            // Due to specular perturbation the resulting specular target could be invalid.
            // This avoids specular rays from going backwards.
            specularTarget += Vector3.Dot(specularTarget, specularTarget + specularPerturbation) >= 0
                                              ? specularPerturbation
                                              : -1.0f * specularPerturbation;

            var target = Vector3.Lerp(diffuseTarget,
                                      specularTarget,
                                      hitRecord.Material.Smoothness);

            var reflectedContribution = proportionalContribution * hitRecord.Material.ReflectiveConstant;

            // We only apply Beer's Law to calculate the ray color if we are inside an object that can transmit light
            TraceRay(new(hitRecord.Point, target),
                     shapeCollection,
                     currentDepth - 1,
                     reflectedContribution,
                     rayColor,
                     ref incomingLight,
                     ref state,
                     applyBeers: applyBeers);
        }

        void ComputeTransmitted(Vector3 rayColor,
                                ref Vector3 incomingLight,
                                ref uint state)
        {
            if (hitRecord.Material.ReflectiveConstant == 1.0f)
            {
                return;
            }

            var refractionRatio = hitRecord.FrontFace
                                      ? AirRefractiveIndex / hitRecord.Material.IndexOfRefraction
                                      : hitRecord.Material.IndexOfRefraction;

            // Snell's law
            var cosTheta = MathF.Min(Vector3.Dot(-1 * ray.Direction, hitRecord.Normal), 1.0f);
            var sinTheta = MathF.Sqrt(1.0f - cosTheta * cosTheta);

            var canRefract = refractionRatio * sinTheta <= 1.0f;

            var target = !canRefract || Reflectance(cosTheta, refractionRatio) > RandomValue(ref state)
                             ? Vector3.Reflect(ray.Direction, hitRecord.Normal)
                             : Refract(ray.Direction, hitRecord.Normal, refractionRatio);

            var transmittedContribution = proportionalContribution * (1 - hitRecord.Material.ReflectiveConstant);

            // We only apply Beer's Law to calculate the ray color if we are inside an object that can transmit light
            TraceRay(new(hitRecord.Point, target),
                     shapeCollection,
                     currentDepth - 1,
                     transmittedContribution,
                     rayColor,
                     ref incomingLight,
                     ref state,
                     applyBeers: !applyBeers);

            #region Local Methods

            // Schlick's approximation for reflectance
            // Source: https://en.wikipedia.org/wiki/Schlick%27s_approximation#:~:text=In%203D%20computer%20graphics%2C%20Schlick's,(surface)%20between%20two%20media
            float Reflectance(float cosTheta, float refractionRatio)
            {
                var rKnot = (1 - refractionRatio) / (1 + refractionRatio);

                rKnot *= rKnot;

                return rKnot + (1 - rKnot) * MathF.Pow((1 - cosTheta), 5);
            }

            #endregion
        }

        #endregion
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
