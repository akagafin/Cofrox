using Cofrox.Domain.Interfaces;

namespace Cofrox.Data.Services;

public sealed class BundledToolLocator : IExternalToolLocator
{
    private readonly string _toolRoot = Path.Combine(AppContext.BaseDirectory, "Tools");

    public string? Resolve(string logicalName)
    {
        IEnumerable<string> candidates = logicalName.ToLowerInvariant() switch
        {
            "ffmpeg" => [Path.Combine(_toolRoot, "ffmpeg", "ffmpeg.exe")],
            "pandoc" => PandocCandidates(),
            "libreoffice" => LibreOfficeCandidates(),
            "magick" => [Path.Combine(_toolRoot, "imagemagick", "magick.exe")],
            "ghostscript" => GhostscriptCandidates(),
            "7zip" => [Path.Combine(_toolRoot, "7zip", "7z.exe")],
            "fonttools" => [Path.Combine(_toolRoot, "fonttools", "fonttools.exe")],
            _ => [],
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    private IEnumerable<string> PandocCandidates()
    {
        yield return Path.Combine(_toolRoot, "pandoc", "pandoc.exe");

        var pandocRoot = Path.Combine(_toolRoot, "pandoc");
        if (!Directory.Exists(pandocRoot))
        {
            yield break;
        }

        foreach (var candidate in Directory.EnumerateFiles(pandocRoot, "pandoc.exe", SearchOption.AllDirectories)
                     .OrderByDescending(static file => File.GetLastWriteTimeUtc(file)))
        {
            yield return candidate;
        }
    }

    private IEnumerable<string> LibreOfficeCandidates()
    {
        yield return Path.Combine(_toolRoot, "libreoffice", "program", "soffice.exe");

        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var standard = Path.Combine(programFiles, "LibreOffice", "program", "soffice.exe");
        if (File.Exists(standard))
        {
            yield return standard;
        }
    }

    private IEnumerable<string> GhostscriptCandidates()
    {
        yield return Path.Combine(_toolRoot, "ghostscript", "gswin64c.exe");

        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var gsRoot = Path.Combine(programFiles, "gs");
        if (!Directory.Exists(gsRoot))
        {
            yield break;
        }

        var installed = new List<string>();
        foreach (var dir in Directory.GetDirectories(gsRoot))
        {
            var candidate = Path.Combine(dir, "bin", "gswin64c.exe");
            if (File.Exists(candidate))
            {
                installed.Add(candidate);
            }
        }

        foreach (var path in installed.OrderByDescending(static f => File.GetLastWriteTimeUtc(f)))
        {
            yield return path;
        }
    }
}
