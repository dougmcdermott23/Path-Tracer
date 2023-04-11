using RayTracer.Shapes;
using System.Numerics;

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
    /// <param name="random">Random number generator</param>
    /// <param name="normal">Unit vector to define hemisphere</param>
    ///
    /// <returns>A point in the unit hemisphere</returns>
    public static Vector3 CosineWeightedDistribution(Random random, Vector3 normal)
        => Vector3.Normalize(normal + RandomInUnitSphere(random));

    /// <summary>
    ///
    ///     Randomly select a point in the unit hemisphere along a normal vector.
    ///
    /// </summary>
    ///
    /// <param name="random">Random number generator</param>
    /// <param name="normal">Unit vector to define hemisphere</param>
    ///
    /// <returns>A point in the unit hemisphere</returns>
    public static Vector3 RandomInUnitHemisphere(Random random, Vector3 normal)
    {
        var target = RandomInUnitSphere(random);

        target *= Vector3.Dot(target, normal) >= 0 ? 1 : -1;

        return target;
    }

    /// <summary>
    ///
    ///     Randomly select a point in the unit sphere. Uses normal distribution so points are uniformly distributed.
    ///
    /// </summary>
    ///
    /// <param name="random">Random number generator</param>
    ///
    /// <returns>A point in the unit sphere</returns>
    public static Vector3 RandomInUnitSphere(Random random)
    {
        var randomX = RandomValueNormalDistribution(random);
        var randomY = RandomValueNormalDistribution(random);
        var randomZ = RandomValueNormalDistribution(random);

        return Vector3.Normalize(new Vector3(randomX, randomY, randomZ));
    }
}
