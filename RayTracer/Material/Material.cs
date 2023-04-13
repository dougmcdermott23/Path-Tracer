using System.ComponentModel.DataAnnotations;
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
    ///     Controls the probability the specular component will be taken into account when calculating the new ray direction.
    ///
    /// </summary>
    public Vector3 SpecularColor { get; init; }

    /// <summary>
    ///
    ///     Emission color of the material. If emission color is Vector3.Zero it does not emit any light.
    ///
    /// </summary>
    public Vector3 EmissionColor { get; init; }

    /// <summary>
    ///
    ///     Controls the proportion of the reflection that is diffuse vs specular. An object with smoothness 1.0f is a perfect mirror.
    ///
    /// </summary>
    [Range(0.0f, 1.0f)]
    public float Smoothness { get; init; }

    /// <summary>
    ///
    ///     Controls the probability the specular component will be taken into account when calculating the new ray direction.
    ///
    /// </summary>
    [Range(0.0f, 1.0f)]
    public float SpecularProbability { get; init; }

    /// <summary>
    ///
    ///     Emission strength of the material. If emission strength is 0 it does not emit any light.
    ///
    /// </summary>
    public float EmissionStrength { get; init; }
}
