using Cofrox.Domain.Interfaces;
using Cofrox.Domain.ValueObjects;

namespace Cofrox.Core.Services;

public sealed class FormatDetectionService(IFormatCatalog catalog)
{
    public FormatDefinition Detect(string path)
    {
        var extension = Path.GetExtension(path);
        var byExtension = catalog.GetByExtension(extension);
        if (!string.Equals(byExtension.Extension, "unknown", StringComparison.OrdinalIgnoreCase))
        {
            return byExtension;
        }

        if (!File.Exists(path))
        {
            return byExtension;
        }

        using var stream = File.OpenRead(path);
        Span<byte> buffer = stackalloc byte[12];
        var read = stream.Read(buffer);
        return DetectByMagicBytes(buffer[..read]) ?? byExtension;
    }

    private FormatDefinition? DetectByMagicBytes(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length >= 4 && bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46)
        {
            return catalog.GetByExtension("pdf");
        }

        if (bytes.Length >= 8 &&
            bytes[0] == 0x89 &&
            bytes[1] == 0x50 &&
            bytes[2] == 0x4E &&
            bytes[3] == 0x47)
        {
            return catalog.GetByExtension("png");
        }

        if (bytes.Length >= 3 && bytes[0] == 0x49 && bytes[1] == 0x44 && bytes[2] == 0x33)
        {
            return catalog.GetByExtension("mp3");
        }

        if (bytes.Length >= 4 && bytes[0] == 0x50 && bytes[1] == 0x4B)
        {
            return catalog.GetByExtension("zip");
        }

        if (bytes.Length >= 4 && bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46)
        {
            return catalog.GetByExtension("wav");
        }

        if (bytes.Length >= 12 &&
            bytes[4] == 0x66 &&
            bytes[5] == 0x74 &&
            bytes[6] == 0x79 &&
            bytes[7] == 0x70)
        {
            return catalog.GetByExtension("mp4");
        }

        return null;
    }
}
