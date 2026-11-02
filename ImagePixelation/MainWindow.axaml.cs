using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System;

namespace ImagePixelation;

public partial class MainWindow : Window
{
    private Bitmap? _bitmap;

    public MainWindow()
    {
        InitializeComponent();

        try
        {
            _bitmap = new Bitmap(Program.Options.FilePath);

            ImageDisplay.Source = _bitmap;

            ImageInfo.Text =
                $"Image: {_bitmap.PixelSize.Width} x {_bitmap.PixelSize.Height} | " +
                $"Square size: {Program.Options.SquareSize} | " +
                $"Mode: {Program.Options.Mode}";
        }
        catch (Exception ex)
        {
            ImageInfo.Text = $"Error loading image: {ex.Message}";
            Console.Error.WriteLine($"Error loading image: {ex.Message}");
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _bitmap?.Dispose();
        base.OnClosed(e);
    }
}
