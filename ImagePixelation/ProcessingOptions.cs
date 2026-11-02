namespace ImagePixelation;

public class ProcessingOptions
{
    public string FilePath { get; }
    public int SquareSize { get; }
    public char Mode { get; }

    public ProcessingOptions(string filePath, int squareSize, char mode)
    {
        FilePath = filePath;
        SquareSize = squareSize;
        Mode = mode;
    }
}
