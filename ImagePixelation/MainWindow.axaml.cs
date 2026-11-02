using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using SkiaSharp;
using System;
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
            ImageInfo.Text = "Processing image...";

            string outputPath = Path.GetFullPath("result.jpg");

            await Task.Run(() =>
            {
                using SKBitmap bitmap = SKBitmap.Decode(Program.Options.FilePath)
                    ?? throw new InvalidOperationException("Unable to decode image.");

                ImageProcessor.Pixelate(bitmap, Program.Options.SquareSize);

                using FileStream stream = File.Create(outputPath);

                if (!bitmap.Encode(stream, SKEncodedImageFormat.Jpeg, 95))
                    throw new InvalidOperationException("Unable to save result.");
            });

            _bitmap?.Dispose();
            _bitmap = new Bitmap(outputPath);

            ImageDisplay.Source = _bitmap;
            ImageInfo.Text = $"Completed! Saved to: {outputPath}";
        }
        catch (Exception ex)
        {
            ImageInfo.Text = $"Error: {ex.Message}";
            Console.Error.WriteLine(ex.Message);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _bitmap?.Dispose();
        base.OnClosed(e);
    }
}
