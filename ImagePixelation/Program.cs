
using Avalonia;
using System;
using System.IO;

namespace ImagePixelation;

class Program
{
    public static ProcessingOptions Options { get; private set; } = null!;

    [STAThread]
    public static int Main(string[] args)
    {
        if (args.Length != 3)
        {
            Console.Error.WriteLine("Usage: ImagePixelation <file.jpg> <squareSize> <S|M>");
            return 1;
        }

        string filePath;

        try
        {
            filePath = Path.GetFullPath(args[0]);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            Console.Error.WriteLine("Error: Invalid file path.");
            return 1;
        }

        if (!File.Exists(filePath))
        {
            Console.Error.WriteLine("Error: Image file not found.");
            return 1;
        }

        string extension = Path.GetExtension(filePath);

        if (!extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) &&
            !extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("Error: Only JPG images are supported.");
            return 1;
        }

        if (!int.TryParse(args[1], out int squareSize) || squareSize <= 0)
        {
            Console.Error.WriteLine("Error: Square size must be a positive integer.");
            return 1;
        }

        string mode = args[2].ToUpperInvariant();

        if (mode != "S" && mode != "M")
        {
            Console.Error.WriteLine("Error: Mode must be S or M.");
            return 1;
        }

        Options = new ProcessingOptions(filePath, squareSize, mode[0]);

        Console.WriteLine($"Image: {Options.FilePath}");
        Console.WriteLine($"Square size: {Options.SquareSize}");
        Console.WriteLine($"Processing mode: {Options.Mode}");

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        return 0;
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
