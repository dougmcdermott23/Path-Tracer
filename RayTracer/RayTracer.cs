﻿using System.Drawing;
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

        var samplesPerPixel = 100;

        var maxDepth = 5;

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
                                            new Sphere(material: new()
                                                                 {
                                                                     MaterialColor = new Vector3(1.0f, 1.0f, 1.0f)
                                                                 },
                                                       center:   new(-2.2f, 0, -1.5f),
                                                       radius:   0.5f),
                                            new Sphere(material: new()
                                                                 {
                                                                     MaterialColor = new Vector3(1.0f, 0.1f, 0.1f)
                                                                 },
                                                       center:   new(-1.1f, 0, -1.5f),
                                                       radius:   0.5f),
                                            new Sphere(material: new()
                                                                 {
                                                                     MaterialColor = new Vector3(0.1f, 1.0f, 0.1f)
                                                                 },
                                                       center:   new(0, 0, -1.5f),
                                                       radius:   0.5f),
                                            new Sphere(material: new()
                                                                 {
                                                                     MaterialColor = new Vector3(0.1f, 0.1f, 1.0f)
                                                                 },
                                                       center:   new(1.1f, 0, -1.5f),
                                                       radius:   0.5f),
                                            new Sphere(material: new()
                                                                 {
                                                                     MaterialColor = new Vector3(1.0f, 1.0f, 1.0f)
                                                                 },
                                                       center:   new(2.2f, 0, -1.5f),
                                                       radius:   0.5f),
                                            new Sphere(material: new()
                                                                 {
                                                                     MaterialColor = new Vector3(0.95f, 0.95f, 0.95f)
                                                                 },
                                                       center:   new(0, -100.5f, -1.0f),
                                                       radius:   100.0f),
                                            new Sphere(material: new()
                                                                 {
                                                                     EmissionColor    = Vector3.One,
                                                                     EmissionStrength = 2.0f
                                                                 },
                                                       center:   new(0, 12.0f, 15.0f),
                                                       radius:   10.0f),
                                        });

        for (var y = 0; y < colorBuffer.Height; y++)
        {
            Console.WriteLine($"Scanlines processed: {y} / {height}");

            for (var x = 0; x < colorBuffer.Width; x++)
            {
                var pixelColor = new Vector3();

                for (var sample = 0; sample < samplesPerPixel; sample++)
                {
                    var u = (float)(x + random.NextDouble()) / colorBuffer.Width;
                    var v = (float)(y + random.NextDouble()) / colorBuffer.Height;

                    var ray = camera.GetRay(u, v);

                    var rayColor      = Vector3.One;
                    var incomingLight = Vector3.Zero;

                    TraceRay(random,
                             ray,
                             world,
                             maxDepth,
                             ref rayColor,
                             ref incomingLight);

                    pixelColor += incomingLight;
                }

                pixelColor /= samplesPerPixel;

                pixelColor = Vector3.Clamp(pixelColor, Vector3.Zero, Vector3.One);

                colorBuffer.WriteColorToBuffer(pixelColor, x, y);
            }
        }

        var bitmap = CreateBitmap(colorBuffer);

        bitmap.Save("raytracer.jpg");
    }

    private static void TraceRay(Random random,
                                 Ray ray,
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

        var target = hitRecord.Value.Point + hitRecord.Value.Normal + RandomInUnitHemisphere(random, hitRecord.Value.Normal);

        var emittedLight = hitRecord.Value.Material.EmissionColor * hitRecord.Value.Material.EmissionStrength;

        incomingLight += emittedLight * rayColor;
        rayColor      *= hitRecord.Value.Material.MaterialColor;

        TraceRay(random,
                 new Ray(hitRecord.Value.Point, target - hitRecord.Value.Point),
                 shapeCollection,
                 currentDepth - 1,
                 ref rayColor,
                 ref incomingLight);
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