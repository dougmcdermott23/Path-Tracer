using System.Numerics;

namespace RayTracer.Material;

public struct Material
{
    /// <summary>
    ///
    ///     Color of the material.
    ///
    /// </summary>
    public Vector3 MaterialColor { get; init; }

    /// <summary>
    ///
    ///     Emission color of the material. If emission color is Vector3.Zero it does not emit any light.
    ///
    /// </summary>
    public Vector3 EmissionColor { get; init; }

    /// <summary>
    ///
    ///     Emission strength of the material. If emission strength is 0 it does not emit any light.
    ///
    /// </summary>
    public float EmissionStrength { get; init; }
}
