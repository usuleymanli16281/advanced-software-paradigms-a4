using Avalonia.Controls;
using Avalonia.Media.Imaging;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.IO;
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
            using SKBitmap decoded = SKBitmap.Decode(Program.Options.FilePath)
                ?? throw new InvalidOperationException("Unable to decode image.");

            using SKBitmap source = decoded.Copy(SKColorType.Bgra8888)
                ?? throw new InvalidOperationException("Unable to convert image.");

            double scale = Math.Min(
                1.0,
                Math.Min(900.0 / source.Width, 650.0 / source.Height));

            int previewWidth = Math.Max(1, (int)(source.Width * scale));
            int previewHeight = Math.Max(1, (int)(source.Height * scale));

            using SKBitmap preview = source.Resize(
                new SKImageInfo(previewWidth, previewHeight),
                new SKSamplingOptions(SKFilterMode.Linear))
                ?? throw new InvalidOperationException("Unable to create preview.");

            using SKCanvas canvas = new SKCanvas(preview);

            int squareSize = Program.Options.SquareSize;
            int rowCount = (int)(
                (source.Height + (long)squareSize - 1) / squareSize);

            int batchCount = Math.Min(8, rowCount);

            ImageInfo.Text = "Processing: 0%";
            ProcessingProgress.Value = 0;

            for (int batch = 0; batch < batchCount; batch++)
            {
                int currentBatch = batch;
                var regions = new ConcurrentQueue<(int Top, int Bottom)>();

                await Task.Run(() =>
                {
                    ImageProcessor.PixelateBatch(
                        source,
                        squareSize,
                        Program.Options.Mode,
                        currentBatch,
                        batchCount,
                        (top, bottom) => regions.Enqueue((top, bottom)));
                });

                while (regions.TryDequeue(out var region))
                {
                    SKRect sourceRect = new SKRect(
                        0, region.Top, source.Width, region.Bottom);

                    SKRect destinationRect = new SKRect(
                        0,
                        (float)(region.Top * scale),
                        previewWidth,
                        (float)(region.Bottom * scale));

                    canvas.DrawBitmap(
                        source,
                        sourceRect,
                        destinationRect,
                        new SKSamplingOptions(SKFilterMode.Linear));
                }

                using SKImage image = SKImage.FromBitmap(preview);
                using SKData data = image.Encode(SKEncodedImageFormat.Jpeg, 85);
                using MemoryStream stream = new MemoryStream(data.ToArray());

                Bitmap updatedBitmap = new Bitmap(stream);

                ImageDisplay.Source = updatedBitmap;

                _bitmap?.Dispose();
                _bitmap = updatedBitmap;

                int progress = (batch + 1) * 100 / batchCount;

                ProcessingProgress.Value = progress;

                ImageInfo.Text =
                    $"Processing: {progress}% | Mode: {Program.Options.Mode}";

                if (batch < batchCount - 1)
                    await Task.Delay(1000);
            }

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

            ProcessingProgress.Value = 100;

            ImageInfo.Text =
                $"Completed | Mode: {Program.Options.Mode} | " +
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
