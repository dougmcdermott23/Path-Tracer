using System.Numerics;

namespace RayTracer;

public struct Ray
{
    public Vector3 Origin { get; }

    public Vector3 Direction { get; }

    public Ray(Vector3 origin, Vector3 direction)
    {
        Origin    = origin;
        Direction = Vector3.Normalize(direction);
    }

    /// <summary>
    ///
    ///     Get the point along the ray at the given distance.
    ///
    /// </summary>
    ///
    /// <param name="distance">Distance from the origin of the array</param>
    ///
    /// <returns>Point along the ray at the given distance</returns>
    public Vector3 PointAt(double distance)
        => Origin + (float)distance * Direction;
}
