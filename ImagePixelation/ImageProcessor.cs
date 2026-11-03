using System;
using System.Threading.Tasks;
using SkiaSharp;

namespace ImagePixelation;

public static class ImageProcessor
{
    public static void PixelateBatch(
        SKBitmap bitmap,
        int squareSize,
        char mode,
        int batch,
        int batchCount,
        Action<int, int>? onRegionCompleted = null)
    {
        if (squareSize <= 0)
            throw new ArgumentException("Square size must be positive.");

        if (bitmap.ColorType != SKColorType.Bgra8888)
            throw new ArgumentException("Bitmap must use BGRA8888 format.");

        if (batchCount <= 0 || batch < 0 || batch >= batchCount)
            throw new ArgumentException("Invalid batch.");

        int rowCount = (int)(
            (bitmap.Height + (long)squareSize - 1) / squareSize);

        if (mode == 'S')
        {
            int start = (int)((long)rowCount * batch / batchCount);
            int end = (int)((long)rowCount * (batch + 1) / batchCount);

            ProcessRows(bitmap, squareSize, start, end);
            ReportRegion(start, end);
        }
        else if (mode == 'M')
        {
            int workerCount = Math.Min(
                Environment.ProcessorCount, rowCount);

            Parallel.For(0, workerCount,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = workerCount
                },
                worker =>
                {
                    int regionStart = worker * rowCount / workerCount;
                    int regionEnd = (worker + 1) * rowCount / workerCount;
                    int regionRows = regionEnd - regionStart;

                    int start = regionStart +
                        (int)((long)regionRows * batch / batchCount);

                    int end = regionStart +
                        (int)((long)regionRows * (batch + 1) / batchCount);

                    ProcessRows(bitmap, squareSize, start, end);
                    ReportRegion(start, end);
                });
        }
        else
        {
            throw new ArgumentException("Invalid processing mode.");
        }

        void ReportRegion(int start, int end)
        {
            if (start >= end)
                return;

            int top = (int)((long)start * squareSize);
            int bottom = (int)Math.Min(
                (long)end * squareSize, bitmap.Height);

            onRegionCompleted?.Invoke(top, bottom);
        }
    }

    private static void ProcessRows(
        SKBitmap bitmap,
        int squareSize,
        int startRow,
        int endRow)
    {
        Span<byte> pixels = bitmap.GetPixelSpan();

        for (int row = startRow; row < endRow; row++)
        {
            int y = (int)((long)row * squareSize);
            int bottom = (int)Math.Min(
                (long)y + squareSize, bitmap.Height);

            for (long x = 0; x < bitmap.Width; x += squareSize)
            {
                ProcessSquare(
                    pixels,
                    bitmap.RowBytes,
                    bitmap.Width,
                    (int)x,
                    y,
                    squareSize,
                    bottom);
            }
        }
    }

    private static void ProcessSquare(
        Span<byte> pixels,
        int stride,
        int width,
        int startX,
        int startY,
        int squareSize,
        int endY)
    {
        int endX = (int)Math.Min(
            (long)startX + squareSize, width);

        long totalBlue = 0;
        long totalGreen = 0;
        long totalRed = 0;
        long count = 0;

        for (int y = startY; y < endY; y++)
        {
            int offset = y * stride + startX * 4;

            for (int x = startX; x < endX; x++)
            {
                totalBlue += pixels[offset];
                totalGreen += pixels[offset + 1];
                totalRed += pixels[offset + 2];

                offset += 4;
                count++;
            }
        }

        byte blue = (byte)(totalBlue / count);
        byte green = (byte)(totalGreen / count);
        byte red = (byte)(totalRed / count);

        for (int y = startY; y < endY; y++)
        {
            int offset = y * stride + startX * 4;

            for (int x = startX; x < endX; x++)
            {
                pixels[offset] = blue;
                pixels[offset + 1] = green;
                pixels[offset + 2] = red;

                offset += 4;
            }
        }
    }
}
