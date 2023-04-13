﻿using System.Numerics;

namespace RayTracer.Utils;

using static MathDM;

public static class VectorUtils
{
    /// <summary>
    ///
    ///     Randomly select a point in the unit hemisphere along a normal vector.
    ///     Point follows the cosine weighted distribution.
    ///
    /// </summary>
    ///
    /// <param name="state">Random number seed</param>
    /// <param name="normal">Unit vector to define hemisphere</param>
    ///
    /// <returns>A point in the unit hemisphere</returns>
    public static Vector3 CosineWeightedDistribution(ref uint state, Vector3 normal)
        => Vector3.Normalize(normal + RandomInUnitSphere(ref state));

    /// <summary>
    ///
    ///     Randomly select a point in the hemisphere along a normal vector.
    ///
    /// </summary>
    ///
    /// <param name="state">Random number seed</param>
    /// <param name="normal">Unit vector to define hemisphere</param>
    /// <param name="radius">Radius of the hemisphere</param>
    ///
    /// <returns>A point in the hemisphere</returns>
    public static Vector3 RandomInHemisphere(ref uint state, Vector3 normal, float radius)
        => RandomInUnitHemisphere(ref state, normal) * radius;

    /// <summary>
    ///
    ///     Randomly select a point in the unit hemisphere along a normal vector.
    ///
    /// </summary>
    ///
    /// <param name="state">Random number seed</param>
    /// <param name="normal">Unit vector to define hemisphere</param>
    ///
    /// <returns>A point in the unit hemisphere</returns>
    public static Vector3 RandomInUnitHemisphere(ref uint state, Vector3 normal)
    {
        var target = RandomInUnitSphere(ref state);

        target *= Vector3.Dot(target, normal) >= 0 ? 1 : -1;

        return target;
    }

    /// <summary>
    ///
    ///     Randomly select a point in the sphere.
    ///
    /// </summary>
    ///
    /// <param name="state">Random number seed</param>
    /// <param name="radius">Radius of the sphere</param>
    ///
    /// <returns>A point in the sphere</returns>
    public static Vector3 RandomInSphere(ref uint state, float radius)
        => RandomInUnitSphere(ref state) * radius;

    /// <summary>
    ///
    ///     Randomly select a point in the unit sphere. Uses normal distribution so points are uniformly distributed.
    ///
    /// </summary>
    ///
    /// <param name="state">Random number seed</param>
    ///
    /// <returns>A point in the unit sphere</returns>
    public static Vector3 RandomInUnitSphere(ref uint state)
    {
        var randomX = RandomValueNormalDistribution(ref state);
        var randomY = RandomValueNormalDistribution(ref state);
        var randomZ = RandomValueNormalDistribution(ref state);

        return Vector3.Normalize(new Vector3(randomX, randomY, randomZ));
    }
}
