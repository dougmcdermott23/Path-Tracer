using System.Numerics;

namespace RayTracer.Shapes;

using Material;

public struct HitRecord
{
    /// <summary>
    ///
    ///     Shape that was hit.
    ///
    /// </summary>
    public IShape Shape { get; init; }

    /// <summary>
    ///
    ///     Point that intersects with the ray and the shape.
    ///
    /// </summary>
    public Vector3 Point { get; init; }

    /// <summary>
    ///
    ///     Normal at the point of intersection.
    ///
    /// </summary>
    public Vector3 Normal { get; init; }

    /// <summary>
    ///
    ///     Root value of the intersection between the ray and the shape at the defined point.
    ///
    /// </summary>
    public double Root { get; init; }

    /// <summary>
    ///
    ///     The side the ray intersected with the shape.
    ///
    /// </summary>
    public bool FrontFace { get; init; }

    /// <summary>
    ///
    ///     Material associated with the shape.
    ///
    /// </summary>
    public Material Material { get; init; }
}
