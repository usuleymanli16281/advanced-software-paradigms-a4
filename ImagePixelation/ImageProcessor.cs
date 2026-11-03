using System;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

namespace ImagePixelation;

public static class ImageProcessor
{
    public static void Pixelate(
        SKBitmap bitmap,
        int squareSize,
        char mode,
        Action<int, int>? onRowCompleted = null)
    {
        int rowCount = (int)(
            (bitmap.Height + (long)squareSize - 1) / squareSize);

        if (mode == 'S')
        {
            ProcessRows(bitmap, squareSize, 0, rowCount, onRowCompleted);
        }
        else if (mode == 'M')
        {
            int workerCount = Math.Min(Environment.ProcessorCount, rowCount);

            Parallel.For(0, workerCount,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = workerCount
                },
                worker =>
                {
                    int startRow = worker * rowCount / workerCount;
                    int endRow = (worker + 1) * rowCount / workerCount;

                    ProcessRows(bitmap, squareSize, startRow, endRow, onRowCompleted);
                });
        }
        else
        {
            throw new ArgumentException("Invalid processing mode.");
        }
    }

    private static void ProcessRows(
        SKBitmap bitmap,
        int squareSize,
        int startRow,
        int endRow,
        Action<int, int>? onRowCompleted)
    {
        for (int row = startRow; row < endRow; row++)
        {
            int y = (int)((long)row * squareSize);
            int bottom = (int)Math.Min((long)y + squareSize, bitmap.Height);

            for (long x = 0; x < bitmap.Width; x += squareSize)
            {
                ProcessSquare(bitmap, (int)x, y, squareSize);
            }

            onRowCompleted?.Invoke(y, bottom);
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
            (byte)(totalBlue / count));

        for (int y = startY; y < endY; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                bitmap.SetPixel(x, y, average);
            }
        }
    }
}
