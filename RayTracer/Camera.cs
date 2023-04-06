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
        LookDirection  = lookDirection;

        CameraRight = Vector3.Cross(lookDirection, Vector3.UnitY);
        CameraUp    = Vector3.Cross(CameraRight, lookDirection);

        LookDirection = Vector3.Normalize(LookDirection);
        CameraRight   = Vector3.Normalize(CameraRight);
        CameraUp      = Vector3.Normalize(CameraUp);
    }
}
