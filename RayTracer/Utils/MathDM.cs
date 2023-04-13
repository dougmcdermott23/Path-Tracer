namespace RayTracer.Utils;

public static class MathDM
{
    /// <summary>
    ///
    ///     Calculate a random number using the normal distribution (mean = 0, std = 1).
    ///
    /// </summary>
    ///
    /// <param name="state">Random number seed</param>
    ///
    /// <returns>Random float</returns>
    public static float RandomValueNormalDistribution(ref uint state)
    {
        var theta = 2 * Math.PI * RandomValue(ref state);

        var rho = MathF.Sqrt(-2 * (float)Math.Log(RandomValue(ref state)));

        return rho * (float)Math.Cos(theta);
    }

    /// <summary>
    ///
    ///     Random number generator. This was faster than using the built in Random library.
    ///
    ///     Source: www.pcg-random.org and www.shadertoy.com/view/XlGcRh
    ///
    /// </summary>
    ///
    /// <param name="state">Random number seed</param>
    ///
    /// <returns>Random number in the range 0 to 1</returns>
    public static float RandomValue(ref uint state)
    {
        state = state * 747796405 + 2891336453;

        var result = ((state >> (int)((state >> 28) + 4)) ^ state) * 277803737;

        result = (result >> 22) ^ result;

        return result / 4294967295.0f;
    }
}
