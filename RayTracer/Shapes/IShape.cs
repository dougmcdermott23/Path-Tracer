using System.Numerics;

namespace RayTracer.Shapes;

public interface IShape
{
    /// <summary>
    ///
    ///     Calculates if a given ray intersects with the shape.
    ///
    /// </summary>
    ///
    /// <param name="ray">The ray that could intersect with the shape</param>
    /// <param name="rootMin">Minimum allowable value for the root</param>
    /// <param name="rootMax">Maximum allowable value for the root</param>
    /// <param name="hitRecord">A record containing hit information or null if there was no intersection</param>
    ///
    /// <returns>If the ray intersects with the shape</returns>
    public bool Hit(Ray ray,
                    double rootMin,
                    double rootMax,
                    out HitRecord? hitRecord);

    /// <summary>
    ///
    ///     Get the normalized normal of the shape at the intersection point.
    ///
    /// </summary>
    ///
    /// <param name="intersectionPoint">Point at which the ray intersected with the shape</param>
    ///
    /// <returns>Normalized normal of the shape at intersection point</returns>
    Vector3 NormalizedNormal(Vector3 intersectionPoint);

    /// <summary>
    ///
    ///     Get the normal of the shape at the intersection point. This vector is not normalized.
    ///
    /// </summary>
    ///
    /// <param name="intersectionPoint">Point at which the ray intersected with the shape</param>
    ///
    /// <returns>Normal of shape at intersection point</returns>
    Vector3 Normal(Vector3 intersectionPoint);
}
