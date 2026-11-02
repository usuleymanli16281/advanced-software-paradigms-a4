using System;
using SkiaSharp;

namespace ImagePixelation;

public static class ImageProcessor
{
    public static void Pixelate(SKBitmap bitmap, int squareSize)
    {
        int width = bitmap.Width;
        int height = bitmap.Height;

        for (int y = 0; y < height; y += squareSize)
        {
            for (int x = 0; x < width; x += squareSize)
            {
                ProcessSquare(bitmap, x, y, squareSize);
            }
        }
    }

    private static void ProcessSquare(
        SKBitmap bitmap, int startX, int startY, int squareSize)
    {
        int endX = (int)Math.Min((long)startX + squareSize, bitmap.Width);
        int endY = (int)Math.Min((long)startY + squareSize, bitmap.Height);

        long totalRed = 0;
        long totalGreen = 0;
        long totalBlue = 0;
        long count = 0;

        for (int y = startY; y < endY; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                SKColor color = bitmap.GetPixel(x, y);

                totalRed += color.Red;
                totalGreen += color.Green;
                totalBlue += color.Blue;
                count++;
            }
        }

        SKColor average = new SKColor(
            (byte)(totalRed / count),
            (byte)(totalGreen / count),
            (byte)(totalBlue / count)
        );

        for (int y = startY; y < endY; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                bitmap.SetPixel(x, y, average);
            }
        }
    }
}
