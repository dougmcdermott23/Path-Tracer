namespace RayTracer.Utils;

public static class MathDM
{
    /// <summary>
    ///
    ///     Calculate a random number using the normal distribution (mean = 0, std = 1).
    ///
    /// </summary>
    ///
    /// <param name="random">Random number generator</param>
    ///
    /// <returns>Random float</returns>
    public static float RandomValueNormalDistribution(Random random)
    {
        var theta = 2 * Math.PI / random.NextDouble();

        var rho = MathF.Sqrt(-2 * (float)Math.Log(random.NextDouble()));

        return rho * (float)Math.Cos(theta);
    }
}
