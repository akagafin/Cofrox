using Cofrox.Domain.Interfaces;

namespace Cofrox.Data.Services;

public sealed class BundledToolLocator : IExternalToolLocator
{
    private readonly string _toolRoot = Path.Combine(AppContext.BaseDirectory, "Tools");

    public string? Resolve(string logicalName)
    {
        string[] candidates = logicalName.ToLowerInvariant() switch
        {
            "ffmpeg" => [Path.Combine(_toolRoot, "ffmpeg", "ffmpeg.exe")],
            "pandoc" => [Path.Combine(_toolRoot, "pandoc", "pandoc.exe")],
            "libreoffice" => [Path.Combine(_toolRoot, "libreoffice", "program", "soffice.exe")],
            "magick" => [Path.Combine(_toolRoot, "imagemagick", "magick.exe")],
            "ghostscript" => [Path.Combine(_toolRoot, "ghostscript", "gswin64c.exe")],
            "7zip" => [Path.Combine(_toolRoot, "7zip", "7z.exe")],
            "fonttools" => [Path.Combine(_toolRoot, "fonttools", "fonttools.exe")],
            _ => [],
        };

        return candidates.FirstOrDefault(File.Exists);
    }
}
