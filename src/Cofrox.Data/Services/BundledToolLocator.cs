using Cofrox.Domain.Interfaces;

namespace Cofrox.Data.Services;

public sealed class BundledToolLocator : IExternalToolLocator
{
    private readonly string _toolRoot = Path.Combine(AppContext.BaseDirectory, "Tools");

    public string? Resolve(string logicalName)
    {
        IEnumerable<string> candidates = logicalName.ToLowerInvariant() switch
        {
            "ffmpeg" => FfmpegCandidates(),
            "pandoc" => PandocCandidates(),
            "libreoffice" => LibreOfficeCandidates(),
            "magick" => ImageMagickCandidates(),
            "ghostscript" => GhostscriptCandidates(),
            "7zip" => SevenZipCandidates(),
            "fonttools" => FontToolsCandidates(),
            _ => [],
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    private IEnumerable<string> FfmpegCandidates()
    {
        yield return Path.Combine(_toolRoot, "ffmpeg", "ffmpeg.exe");

        foreach (var candidate in CandidatesFromPath("ffmpeg.exe"))
        {
            yield return candidate;
        }

        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        yield return Path.Combine(programFiles, "FFmpeg", "bin", "ffmpeg.exe");
        yield return Path.Combine(programFiles, "ffmpeg", "bin", "ffmpeg.exe");
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

        foreach (var candidate in CandidatesFromPath("pandoc.exe"))
        {
            yield return candidate;
        }

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        yield return Path.Combine(localAppData, "Pandoc", "pandoc.exe");

        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        yield return Path.Combine(programFiles, "Pandoc", "pandoc.exe");
    }

    private IEnumerable<string> LibreOfficeCandidates()
    {
        yield return Path.Combine(_toolRoot, "libreoffice", "program", "soffice.exe");

        foreach (var candidate in CandidatesFromPath("soffice.exe"))
        {
            yield return candidate;
        }

        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var standard = Path.Combine(programFiles, "LibreOffice", "program", "soffice.exe");
        if (File.Exists(standard))
        {
            yield return standard;
        }
    }

    private IEnumerable<string> ImageMagickCandidates()
    {
        yield return Path.Combine(_toolRoot, "imagemagick", "magick.exe");

        foreach (var candidate in CandidatesFromPath("magick.exe"))
        {
            yield return candidate;
        }

        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        if (!Directory.Exists(programFiles))
        {
            yield break;
        }

        foreach (var dir in Directory.EnumerateDirectories(programFiles, "ImageMagick*", SearchOption.TopDirectoryOnly)
                     .OrderByDescending(static path => path, StringComparer.OrdinalIgnoreCase))
        {
            yield return Path.Combine(dir, "magick.exe");
        }
    }

    private IEnumerable<string> GhostscriptCandidates()
    {
        yield return Path.Combine(_toolRoot, "ghostscript", "gswin64c.exe");

        foreach (var candidate in CandidatesFromPath("gswin64c.exe"))
        {
            yield return candidate;
        }

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

    private IEnumerable<string> SevenZipCandidates()
    {
        yield return Path.Combine(_toolRoot, "7zip", "7z.exe");

        foreach (var candidate in CandidatesFromPath("7z.exe"))
        {
            yield return candidate;
        }

        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        yield return Path.Combine(programFiles, "7-Zip", "7z.exe");
    }

    private IEnumerable<string> FontToolsCandidates()
    {
        yield return Path.Combine(_toolRoot, "fonttools", "fonttools.exe");

        foreach (var candidate in CandidatesFromPath("fonttools.exe"))
        {
            yield return candidate;
        }
    }

    private static IEnumerable<string> CandidatesFromPath(string executableName)
    {
        var pathVariable = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathVariable))
        {
            yield break;
        }

        foreach (var segment in pathVariable.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            yield return Path.Combine(segment, executableName);
        }
    }
}
