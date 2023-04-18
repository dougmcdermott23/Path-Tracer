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
    ///     Radius of sphere to randomize the direction of the reflected ray. The larger the radius, the fuzzier the reflection.
    ///
    /// </summary>
    public float Fuzziness { get; init; }

    /// <summary>
    ///
    ///     Defines how much the path of light is refracted when entering the material.
    ///
    /// </summary>
    public float IndexOfRefraction { get; init; }

    /// <summary>
    ///
    ///     Emission strength of the material. If emission strength is 0 it does not emit any light.
    ///
    /// </summary>
    public float EmissionStrength { get; init; }

    /// <summary>
    ///
    ///     Defines the proprtion of the color that is attributed due to reflection vs transmission.
    ///     A shape with a constant 1.0f will only reflect while a shape with a constant of 0.0f will only refract.
    ///
    /// </summary>
    [Range(0.0f, 1.0f)]
    public float ReflectiveConstant { get; init; }
}
