using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;

namespace RayTracer;

using Shapes;

using static Constants;

public class RayTracer
{
    private const int BytesPerColor = 3; // Color in buffer shall be stored as RGB

    private const float AspectRatio = 16.0f / 9.0f;

    public static void Main(string[] args)
    {
        var width  = 400;
        var height = (int)(width / AspectRatio);

        var buffer = new byte[width * height * BytesPerColor];

        var viewportHeight = 2.0f;
        var viewportWidth  = AspectRatio * viewportHeight;
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

        for (var y = 0; y < height; y++)
        {
            Console.WriteLine($"Scanlines processed: {y} / {height}");

            for (var x = 0; x < width; x++)
            {
                var u = (float)x / (width - 1);
                var v = (float)y / (height - 1);

                var rayTarget = camera.BotLeftViewport
                                + u * camera.ViewportWidth * camera.CameraRight
                                + v * camera.ViewportHeight * camera.CameraUp;

                var rayDirection = rayTarget - camera.Origin;

                var ray = new Ray(camera.Origin, rayDirection);

                var rayColor = GetRayColor(ray, world);

                var bufferIndex = BytesPerColor * (y * width + x);

                buffer[bufferIndex]     = (byte)(rayColor.X * 255);
                buffer[bufferIndex + 1] = (byte)(rayColor.Y * 255);
                buffer[bufferIndex + 2] = (byte)(rayColor.Z * 255);

                #region Local Methods

                Vector3 GetRayColor(Ray ray, ShapeCollection shapeCollection)
                {
                    if (world.Hit(ray, MinimumRoot, MaximumRoot, out var hitRecord))
                    {
                        var normal = hitRecord?.Normal ?? throw new ArgumentException("Normal shall not be null if there is an intersection.");

                        return 0.5f * (normal + new Vector3(1, 1, 1));
                    }

                    var mod = 0.5f * (ray.Direction.Y + 1.0f);

                    return (1.0f - mod) * new Vector3(1.0f, 1.0f, 1.0f) + mod * new Vector3(0.5f, 0.7f, 1.0f);
                }

                #endregion
            }
        }

        var bitmap = CreateBitmap(width, height, buffer);

        bitmap.Save("raytracer.jpg");
    }

    private static Bitmap CreateBitmap(int width, int height, byte[] buffer)
    {
        if (buffer.Length != width * height * BytesPerColor)
        {
            throw new ArgumentException("Length of buffer does not match picture size.");
        }

        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppRgb);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var bufferIndex = BytesPerColor * (y * width + x);

                var color = Color.FromArgb(buffer[bufferIndex],
                                           buffer[bufferIndex + 1],
                                           buffer[bufferIndex + 2]);

                // Camera viewport uses uv coordiantes where 0,0 is bottom left
                // The y coordinate must be inversed as 0,0 is the top left for Bitmap
                bitmap.SetPixel(x, height - 1 - y, color);
            }
        }

        return bitmap;
    }
}