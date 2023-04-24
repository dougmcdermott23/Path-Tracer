using System.Numerics;

namespace RayTracer.Shapes;

using Material;

using static Constants;

public class Sphere : IShape
{
    /// <summary>
    ///
    ///     Material associated with the sphere.
    ///
    /// </summary>
    public Material Material { get; set; }

    /// <summary>
    ///
    ///     Center point of the sphere.
    ///
    /// </summary>
    public Vector3 Center { get; set; }

    /// <summary>
    ///
    ///     Radius of the sphere.
    ///
    /// </summary>
    public float Radius { get; set; }

    public Sphere(Material material,
                  Vector3 center,
                  float radius)
    {
        Material = material;
        Center   = center;
        Radius   = radius;
    }

    /// <summary>
    ///
    ///     Given the equation below, a ray intersects with a sphere:
    ///         - Twice if there are two roots;
    ///         - Once if there is one root; and
    ///         - Does not intersect if there are zero roots.
    ///
    ///     Equation of a sphere in vector form is: (P − C)⋅(P − C) = r^2
    ///     The ray can be defined as:              P(t) = A + tb
    ///
    ///     If the ray intersects with the sphere, there is exists some t which satisfies the following:
    ///     (t^2 * b⋅b + 2 * t* b⋅(A − C) + (A − C)⋅(A−C) − r^2=0
    ///
    /// </summary>
    ///
    /// <param name="ray">The ray that could intersect with the sphere</param>
    /// <param name="rootMin">Minimum allowable value for the root</param>
    /// <param name="rootMax">Maximum allowable value for the root</param>
    /// <param name="hitRecord">A record containing hit information or null if there was no intersection</param>
    ///
    /// <returns>If the ray intersects with the sphere</returns>
    public bool Hit(Ray ray,
                    double rootMin,
                    double rootMax,
                    out HitRecord hitRecord)
    {
        var oc     = ray.Origin - Center;
        var a      = Vector3.Dot(ray.Direction, ray.Direction);
        var halfB  = Vector3.Dot(oc, ray.Direction);
        var c      = Vector3.Dot(oc, oc) - Radius * Radius;

        var disciminant = halfB * halfB - a * c;

        if (disciminant < 0)
        {
            hitRecord = EmptyHitRecord;

            return false;
        }

        // find the nearest root that lies in the acceptable range
        var sqrtDiscriminant = MathF.Sqrt(disciminant);
        var root             = (-halfB - sqrtDiscriminant) / a;

        if (root < rootMin || root > rootMax)
        {
            root = (-halfB + sqrtDiscriminant) / a;

            if (root < rootMin || root > rootMax)
            {
                hitRecord = EmptyHitRecord;

                return false;
            }
        }

        var point     = ray.PointAt(root);
        var normal    = (point - Center) / Radius;
        var frontFace = Vector3.Dot(ray.Direction, normal) < 0;

        hitRecord = new()
                    {
                        Shape     = this,
                        Point     = point,
                        Normal    = frontFace ? normal : -normal,
                        Root      = root,
                        FrontFace = frontFace,
                        Material  = Material
                    };

        return true;
    }

    /// <summary>
    ///
    ///     Get the normalized normal of the sphere at the intersection point.
    ///
    /// </summary>
    ///
    /// <param name="intersectionPoint">Point at which the ray intersected with the sphere</param>
    ///
    /// <returns>Normalized normal of the sphere at intersection point</returns>
    public Vector3 NormalizedNormal(Vector3 intersectionPoint)
        => Vector3.Normalize(Normal(intersectionPoint));

    /// <summary>
    ///
    ///     Get the normal of the sphere at the intersection point. This vector is not normalized.
    ///
    /// </summary>
    ///
    /// <param name="intersectionPoint">Point at which the ray intersected with the sphere</param>
    ///
    /// <returns>Normal of sphere at intersection point</returns>
    public Vector3 Normal(Vector3 intersectionPoint)
        => intersectionPoint - Center;
}
