using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;

namespace RayTracer;

using Shapes;

using static Constants;
using static Utils.VectorUtils;

public class RayTracer
{
    public static void Main(string[] args)
    {
        var aspectRatio = 16.0f / 9.0f;
        var width       = 400;
        var height      = (int)(width / aspectRatio);

        var colorBuffer = new ColorBuffer(width, height);

        var samplesPerPixel    = 100;
        var invSamplesPerPixel = 1 / (float)samplesPerPixel;

        var depth = 10;

        var random = new Random();

        var viewportHeight = 2.0f;
        var viewportWidth  = aspectRatio * viewportHeight;
        var focalLength    = 1.0f;
        var origin         = new Vector3(0, 0, 0);
        var lookDirection  = new Vector3(0, 0, -1);

        var camera = new Camera(viewportHeight,
                                viewportWidth,
                                focalLength,
                                origin,
                                lookDirection);

        var world = new ShapeCollection(new()
                                        {
                                            new Sphere(center: new(0, 0, -1),
                                                               radius: 0.5f),
                                            new Sphere(center: new(0, -100.5f, -1),
                                                               radius: 100.0f)
                                        });

        for (var y = 0; y < colorBuffer.Height; y++)
        {
            Console.WriteLine($"Scanlines processed: {y} / {height}");

            for (var x = 0; x < colorBuffer.Width; x++)
            {
                var rayColor = new Vector3();

                for (var sample = 0; sample < samplesPerPixel; sample++)
                {
                    var u = (float)(x + random.NextDouble()) / colorBuffer.Width;
                    var v = (float)(y + random.NextDouble()) / colorBuffer.Height;

                    var ray = camera.GetRay(u, v);

                    rayColor += GetRayColor(ray, world, depth);
                }

                rayColor *= invSamplesPerPixel;

                colorBuffer.WriteColorToBuffer(rayColor, x, y);

                #region Local Methods

                Vector3 GetRayColor(Ray ray, ShapeCollection shapeCollection, int depth)
                {
                    if (depth <= 0)
                    {
                        return Vector3.Zero;
                    }

                    if (shapeCollection.Hit(ray, MinimumRoot, MaximumRoot, out var hitRecord))
                    {
                        if (hitRecord is null)
                        {
                            throw new ArgumentException("Hit record shall not be null if there is a hit detected.");
                        }

                        var diffuseTarget = RandomInUnitSphere(random);

                        diffuseTarget *= Vector3.Dot(diffuseTarget, hitRecord.Value.Normal) >= 0 ? 1 : -1;

                        var target = hitRecord.Value.Point + hitRecord.Value.Normal + diffuseTarget;

                        return 0.5f * GetRayColor(new Ray(hitRecord.Value.Point, target - hitRecord.Value.Point), shapeCollection, depth - 1);
                    }

                    var mod = 0.5f * (ray.Direction.Y + 1.0f);

                    return (1.0f - mod) * new Vector3(1.0f, 1.0f, 1.0f) + mod * new Vector3(0.5f, 0.7f, 1.0f);
                }

                #endregion
            }
        }

        var bitmap = CreateBitmap(colorBuffer);

        bitmap.Save("raytracer.jpg");
    }

    private static Bitmap CreateBitmap(ColorBuffer colorBuffer)
    {
        var bitmap = new Bitmap(colorBuffer.Width, colorBuffer.Height, PixelFormat.Format32bppRgb);

        for (var y = 0; y < colorBuffer.Height; y++)
        {
            for (var x = 0; x < colorBuffer.Width; x++)
            {
                var color = colorBuffer.ReadColorFromBuffer(x, y);

                // Camera viewport uses uv coordiantes where 0,0 is bottom left
                // The y coordinate must be inversed as 0,0 is the top left for Bitmap
                bitmap.SetPixel(x, colorBuffer.Height - 1 - y, color);
            }
        }

        return bitmap;
    }
}