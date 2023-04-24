using System.Numerics;

namespace RayTracer;

using Shapes;

public static class Constants
{
    public const double MinimumRoot        = 0.001; // Set to a value > 0 to avoid self intersections

    public const double MaximumRoot        = Double.PositiveInfinity;

    public const float SkinWidth           = 0.001f; // Set to a value > 0 to avoid self intersections

    public const float AirRefractiveIndex  = 1.0f;

    public static readonly HitRecord EmptyHitRecord = new();

    public static readonly Vector3 DefaultSkyboxMin = Vector3.One;

    public static readonly Vector3 DefaultSkyboxMax = new(0.5f, 0.7f, 1.0f);
}
