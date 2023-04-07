using System.Numerics;

namespace RayTracer.Utils;

using static MathDM;

public static class VectorUtils
{
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
