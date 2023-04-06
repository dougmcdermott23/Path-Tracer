namespace RayTracer.Shapes;

public class ShapeCollection
{
    public List<IShape> ShapeList { get; set; } = new();

    public ShapeCollection()
    {
    }

    public ShapeCollection(List<IShape> shapeList)
        => ShapeList = shapeList;

    /// <summary>
    ///
    ///     Finds the nearest shape in the shape list that intersects with the ray.
    ///
    /// </summary>
    ///
    /// <param name="ray">The ray that could intersect with the shape</param>
    /// <param name="rootMin">Minimum allowable value for the root</param>
    /// <param name="rootMax">Maximum allowable value for the root</param>
    /// <param name="hitRecord">A record containing hit information or null if there was no intersection</param>
    ///
    /// <returns>If the ray intersects with a shape in the shape list</returns>
    ///
    /// <exception cref="ArgumentException">Thrown if a hit was detected but no hit record was initialized</exception>
    public bool Hit(Ray ray,
                    double rootMin,
                    double rootMax,
                    out HitRecord? hitRecord)
    {
        var hitDetected = false;
        var closestHit  = rootMax;

        hitRecord = null;

        foreach (var shape in ShapeList)
        {
            if (shape.Hit(ray, rootMin, closestHit, out var shapeHitRecord))
            {
                hitDetected = true;
                closestHit  = shapeHitRecord?.Root ?? throw new ArgumentException("Normal shall not be null if there is an intersection.");
                hitRecord   = shapeHitRecord;
            }
        }

        return hitDetected;
    }
}
