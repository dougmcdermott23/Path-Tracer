using RayTracer.Shapes;

namespace RayTracer;

public static class Constants
{
    public const double MinimumRoot        = 0.001; // Set to a value > 0 to avoid self intersections

    public const double MaximumRoot        = Double.PositiveInfinity;

    public const float SkinWidth           = 0.001f; // Set to a value > 0 to avoid self intersections

    public static HitRecord EmptyHitRecord = new();
}
