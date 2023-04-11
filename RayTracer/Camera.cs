using System.Numerics;

namespace RayTracer;

public class Camera
{
    public float ViewportHeight { get; }

    public float ViewportWidth { get; }

    public float FocalLength { get; }

    public Vector3 Origin { get; }

    public Vector3 LookDirection { get; }

    public Vector3 CameraRight { get; }

    public Vector3 CameraUp { get; }

    public Vector3 BotLeftViewport
        => Origin - ViewportWidth * CameraRight / 2.0f - ViewportHeight * CameraUp / 2.0f + FocalLength * LookDirection;

    public Vector3 BotRightViewport
        => Origin + ViewportWidth * CameraRight / 2.0f - ViewportHeight * CameraUp / 2.0f + FocalLength * LookDirection;

    public Vector3 TopLeftViewport
        => Origin - ViewportWidth * CameraRight / 2.0f + ViewportHeight * CameraUp / 2.0f + FocalLength * LookDirection;

    public Vector3 TopRightViewport
        => Origin + ViewportWidth * CameraRight / 2.0f + ViewportHeight * CameraUp / 2.0f + FocalLength * LookDirection;

    public Camera(float viewportHeight,
                  float viewportWidth,
                  float focalLength,
                  Vector3 origin,
                  Vector3 lookDirection)
    {
        ViewportHeight = viewportHeight;
        ViewportWidth  = viewportWidth;
        FocalLength    = focalLength;
        Origin         = origin;
        LookDirection  = Vector3.Normalize(lookDirection);

        CameraRight = Vector3.Cross(lookDirection, Vector3.UnitY);
        CameraUp    = Vector3.Cross(CameraRight, lookDirection);

        LookDirection = Vector3.Normalize(LookDirection);
        CameraRight   = Vector3.Normalize(CameraRight);
        CameraUp      = Vector3.Normalize(CameraUp);
    }

    /// <summary>
    ///
    ///     Return a ray starting at the camera orgin towards the u, v coordinate on the viewport.
    ///
    /// </summary>
    ///
    /// <param name="u">Horizontal coordinate on viewport between 0 and 1</param>
    /// <param name="v">Vertical coordinate on viewport between 0 and 1</param>
    ///
    /// <returns>Ray at origin passing through u, v on viewport</returns>
    public Ray GetRay(float u, float v)
    {
        var rayTarget = BotLeftViewport + u * ViewportWidth * CameraRight + v * ViewportHeight * CameraUp;

        var rayDirection = rayTarget - Origin;

        return new(Origin, rayDirection);
    }
}
