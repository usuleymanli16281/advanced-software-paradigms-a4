using Avalonia.Controls;
using Avalonia.Media.Imaging;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImagePixelation;

public partial class MainWindow : Window
{
    private Bitmap? _bitmap;

    public MainWindow()
    {
        InitializeComponent();

        _bitmap = new Bitmap(Program.Options.FilePath);
        ImageDisplay.Source = _bitmap;

        ImageInfo.Text =
            $"Image: {_bitmap.PixelSize.Width} x {_bitmap.PixelSize.Height} | " +
            $"Square size: {Program.Options.SquareSize} | " +
            $"Mode: {Program.Options.Mode}";

        Opened += async (_, _) => await ProcessImageAsync();
    }

    private async Task ProcessImageAsync()
    {
        try
        {
            using SKBitmap source = SKBitmap.Decode(Program.Options.FilePath)
                ?? throw new InvalidOperationException("Unable to decode image.");

            using SKBitmap preview = source.Copy()
                ?? throw new InvalidOperationException("Unable to create preview.");

            object previewLock = new object();

            int squareSize = Program.Options.SquareSize;
            int totalRows = (int)(
                (source.Height + (long)squareSize - 1) / squareSize);

            int completedRows = 0;

            ImageInfo.Text = "Processing: 0%";

            Task<TimeSpan> processingTask = Task.Run(() =>
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                ImageProcessor.Pixelate(
                    source,
                    squareSize,
                    Program.Options.Mode,
                    (startY, endY) =>
                    {
                        lock (previewLock)
                        {
                            for (int y = startY; y < endY; y++)
                            {
                                for (int x = 0; x < source.Width; x++)
                                {
                                    preview.SetPixel(x, y, source.GetPixel(x, y));
                                }
                            }
                        }

                        Interlocked.Increment(ref completedRows);
                    });

                stopwatch.Stop();
                return stopwatch.Elapsed;
            });

            while (!processingTask.IsCompleted)
            {
                await Task.WhenAny(processingTask, Task.Delay(250));

                if (processingTask.IsCompleted)
                    break;

                byte[] previewBytes = await Task.Run(() =>
                {
                    lock (previewLock)
                    {
                        using SKImage image = SKImage.FromBitmap(preview);
                        using SKData data = image.Encode(
                            SKEncodedImageFormat.Jpeg, 80);

                        return data.ToArray();
                    }
                });

                using MemoryStream stream = new MemoryStream(previewBytes);

                Bitmap updatedBitmap = new Bitmap(stream);

                ImageDisplay.Source = updatedBitmap;

                _bitmap?.Dispose();
                _bitmap = updatedBitmap;

                int progress = Volatile.Read(ref completedRows) * 100 / totalRows;

                ImageInfo.Text = $"Processing: {progress}% | Mode: {Program.Options.Mode}";
            }

            TimeSpan elapsed = await processingTask;

            string outputPath = Path.GetFullPath("result.jpg");

            await Task.Run(() =>
            {
                using FileStream stream = File.Create(outputPath);

                if (!source.Encode(stream, SKEncodedImageFormat.Jpeg, 95))
                    throw new InvalidOperationException("Unable to save result.");
            });

            Bitmap finalBitmap = new Bitmap(outputPath);

            ImageDisplay.Source = finalBitmap;

            _bitmap?.Dispose();
            _bitmap = finalBitmap;

            ImageInfo.Text =
                $"Completed in {elapsed.TotalSeconds:F2}s | " +
                $"Mode: {Program.Options.Mode} | " +
                $"Saved to: {outputPath}";
        }
        catch (Exception ex)
        {
            ImageInfo.Text = $"Error: {ex.Message}";
            Console.Error.WriteLine(ex);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _bitmap?.Dispose();
        base.OnClosed(e);
    }
}
