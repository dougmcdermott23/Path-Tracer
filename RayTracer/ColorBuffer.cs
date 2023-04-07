using System.Drawing;
using System.Numerics;

namespace RayTracer;

public class ColorBuffer
{
    private const int BytesPerColor = 3; // Color in buffer shall be stored as RGB

    private byte[] Buffer { get; }

    /// <summary>
    ///
    ///     Width of the image.
    ///
    /// </summary>
    public int Width { get; }

    /// <summary>
    ///
    ///     Height of the image.
    ///
    /// </summary>
    public int Height { get; }

    public ColorBuffer(int width, int height)
    {
        Width  = width;
        Height = height;

        Buffer = new byte[width * height * BytesPerColor];
    }

    /// <summary>
    ///
    ///     Save a color to the buffer at the given image coordinate.
    ///
    /// </summary>
    ///
    /// <param name="color">3D vector containing the color values (0 to 1.0)</param>
    ///
    /// <param name="x">Horizontal coordinate in the image</param>
    /// <param name="y">Vertical coordinate in the image</param>
    public void WriteColorToBuffer(Vector3 color, int x, int y)
    {
        var bufferIndex = GetBufferIndex(x, y);

        Buffer[bufferIndex]     = (byte)(color.X * 255);
        Buffer[bufferIndex + 1] = (byte)(color.Y * 255);
        Buffer[bufferIndex + 2] = (byte)(color.Z * 255);
    }

    /// <summary>
    ///
    ///     Returns a color stored in the buffer at the given image coordinate.
    ///
    /// </summary>
    ///
    /// <param name="x">Horizontal coordinate in the image</param>
    /// <param name="y">Vertical coordinate in the image</param>
    ///
    /// <returns>Color with 8-bit color values</returns>
    public Color ReadColorFromBuffer(int x, int y)
    {
        var bufferIndex = GetBufferIndex(x, y);

        return Color.FromArgb(Buffer[bufferIndex],
                              Buffer[bufferIndex + 1],
                              Buffer[bufferIndex + 2]);
    }

    private int GetBufferIndex(int x, int y)
        => BytesPerColor * (y * Width + x);
}
