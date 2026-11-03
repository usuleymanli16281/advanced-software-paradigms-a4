# Image Pixelation — Assignment 4

A cross-platform image pixelation application developed using C# (.NET 9), Avalonia UI, and SkiaSharp.

The application divides a JPG image into square regions and replaces each region with its average color. It support single-threaded and multi-threaded processing with progressive visualization.

## Features

- Accepts an image filename, square size, and processing mode through command-line arguments.
- Calculates the average RGB color of each square region.
- Supports single-threaded (`S`) and multi-threaded (`M`) processing.
- Uses available CPU cores for parallel processing.
- Displays processing progress in a desktop window.
- Saves the processed image as `result.jpg`.
- Supports Windows x64 and macOS Apple Silicon (ARM64).

## Running the Application

The repository includes launchers that automatically download precompiled applications from [GitHub Releases](https://github.com/usuleymanli16281/advanced-software-paradigms-a4/releases/tag/v1.0.0).

**No .NET SDK or Runtime installation is required.**

An internet connection is required for the initial download.

### Clone the Repository

```bash
git clone https://github.com/usuleymanli16281/advanced-software-paradigms-a4.git
cd advanced-software-paradigms-a4
```

### Windows

Open PowerShell in the repository directory.

Single-threaded mode:

```powershell
.\run-windows.cmd photo.jpg 80 S
```

Multi-threaded mode:

```powershell
.\run-windows.cmd photo.jpg 80 M
```

### macOS (Apple Silicon)

Open Terminal in the repository directory.

Single-threaded mode:

```bash
sh run-mac.sh photo.jpg 80 S
```

Multi-threaded mode:

```bash
sh run-mac.sh photo.jpg 80 M
```

The macOS launcher applies an ad-hoc code signature to the downloaded executable. macOS may still require security approval.

### Command-Line Arguments

| Argument | Description | Example |
|----------|-------------|---------|
| Image filename | Path to a JPG image | `photo.jpg` |
| Square size | Positive integer specifying the pixelation square size | `80` |
| Processing mode | `S` (single-threaded) or `M` (multi-threaded) | `M` |

The specified image must exist. You can place a JPG file in the repository directory or provide its absolute path.

For example, on Windows:

```powershell
.\run-windows.cmd "C:\Users\Student\Pictures\image.jpg" 40 M
```

On macOS:

```bash
sh run-mac.sh "$HOME/Pictures/image.jpg" 40 M
```

## Implementation

The application divides the image into square regions according to the specified square size.

For each region:

1. Calculate the average red, green, and blue values of its pixels.
2. Replace all pixels in the region with the calculated average color.
3. Update the displayed image progressively.

Regions along the image boundaries are processed using their remaining dimensions.

### Single-Threaded Mode (S)

Processes image regions sequentially, from top to bottom and left to right.

### Multi-Threaded Mode (M)

Uses `Parallel.For` to process independent image regions concurrently, distributing work across available CPU cores.

Both modes apply the same algorithm and produce equivalent pixelation results.

## Output

The processed image is saved as:

```text
result.jpg
```

The file is created in the repository directory. Running the application again overwrites the previous result.

## Technologies

- C# / .NET 9
- Avalonia UI
- SkiaSharp
- Task Parallel Library (`Parallel.For`)


## References

- [SkiaSharp Tutorial — SWHarden](https://swharden.com/csdv/skiasharp/skiasharp/) — Reference for image processing and graphics using SkiaSharp.
- [Avalonia UI Documentation](https://docs.avaloniaui.net/docs/welcome) — Official documentation for building cross-platform desktop applications with Avalonia UI.
