using System.Numerics;

namespace RayTracer;

using Shapes;

using static Utils.MathDM;
using static Utils.VectorUtils;

public class Program
{
    public static void Main(string[] args)
    {
        var aspectRatio = 16.0f / 9.0f;
        var width       = 1920;
        var height      = (int)(width / aspectRatio);

        var colorBuffer = new ColorBuffer(width, height);

        var viewportHeight  = 3.0f;
        var viewportWidth   = aspectRatio * viewportHeight;
        var focalLength     = 10.0f;
        var defocusStrength = 0.35f;
        var origin          = new Vector3(13.0f, 2.0f, 3.0f);
        var lookDirection   = new Vector3(1.0f, 0.5f, 0.0f) - origin;

        var camera = new Camera(viewportHeight,
                                viewportWidth,
                                focalLength,
                                defocusStrength,
                                origin,
                                lookDirection);

        var world = RandomScene();

        var maxDepth = 7;

        var samplesPerPixel = 500;

        var rayTracer = new RayTracer(camera,
                                      colorBuffer,
                                      world,
                                      maxDepth,
                                      samplesPerPixel,
                                      Environment.ProcessorCount,
                                      true);

        rayTracer.Run();
    }

    private static ShapeCollection RandomScene()
    {
        var shapeList = new List<IShape>()
        {
            // Ground
            new Sphere(material: new()
                                 {
                                     MaterialColor      = new Vector3(0.95f, 0.95f, 0.95f),
                                     ReflectiveConstant = 1.0f
                                 },
                       center:   new(0.0f, -1000.0f, 0.0f),
                       radius:   1000.0f),
            // Light
            new Sphere(material: new()
                                 {
                                     EmissionColor    = Vector3.One,
                                     EmissionStrength = 2.0f
                                 },
                       center:   new(-5.0f, 10.0f, 10.0f),
                       radius:   5.0f),
            new Sphere(material: new()
                                 {
                                     MaterialColor      = new(0.4f, 0.2f, 0.1f),
                                     Smoothness         = 0.0f,
                                     ReflectiveConstant = 1.0f
                                 },
                       center:   new(-4.0f, 1.0f, 0.0f),
                       radius:   1.0f),
            new Sphere(material: new()
                                 {
                                     MaterialColor      = new(0.7f, 0.6f, 0.5f),
                                     Smoothness         = 1.0f,
                                     ReflectiveConstant = 1.0f
                                 },
                       center:   new(0.0f, 1.0f, 0.0f),
                       radius:   1.0f),
            new Sphere(material: new()
                                 {
                                     MaterialColor      = Vector3.One,
                                     ReflectiveConstant = 0.0f,
                                     IndexOfRefraction  = 1.5f
                                 },
                       center:   new(4.0f, 1.0f, 0.0f),
                       radius:   1.0f)
        };

        var random = new Random();
        var state  = (uint)DateTime.Now.Millisecond;

        for (var x = -11; x < 11; x++)
        {
            for (var z = -7; z < 4; z++)
            {
                var center = new Vector3(x + 0.9f * (float)RandomValue(ref state), 0.2f, z + 0.9f * (float)RandomValue(ref state));

                if (Vector3.Distance(center, new Vector3(4.0f, 0.2f, 0.0f)) <= 0.9f)
                {
                    continue;
                }

                Material.Material material;

                var chooseMaterial = RandomValue(ref state);

                // Lambertian
                if (chooseMaterial < 0.8)
                {
                    material = new()
                               {
                                   MaterialColor      = RandomVector(ref state) * RandomVector(ref state),
                                   Smoothness         = 0.0f,
                                   ReflectiveConstant = 1.0f
                               };
                }
                // Metal
                else if (chooseMaterial < 0.9)
                {
                    material = new()
                               {
                                   MaterialColor      = RandomVector(ref state) * RandomVector(ref state),
                                   Smoothness         = random.Next(5, 10) / 10.0f,
                                   Fuzziness          = random.Next(5) / 10.0f,
                                   ReflectiveConstant = 1.0f
                               };
                }
                // Glass
                else
                {
                    material = new()
                               {
                                   MaterialColor      = RandomVector(ref state),
                                   ReflectiveConstant = 0.0f,
                                   IndexOfRefraction  = 1.0f + random.Next(6) / 10.0f,
                                   Absorbance         = random.Next(0, 8)
                               };
                }

                shapeList.Add(new Sphere(material: material,
                                         center:   center,
                                         radius:   0.2f));
            }
        }

        return new(shapeList);
    }
}