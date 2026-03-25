using Cofrox.Domain.Interfaces;

namespace Cofrox.Data.Services;

public sealed class TempFileService : ITempFileService
{
    public TempFileService()
    {
        RootPath = Path.Combine(Path.GetTempPath(), "Cofrox");
        Directory.CreateDirectory(RootPath);
    }

    public string RootPath { get; }

    public string CreateJobFolder(string jobId)
    {
        var folder = Path.Combine(RootPath, jobId);
        Directory.CreateDirectory(folder);
        return folder;
    }

    public Task CleanupStaleFilesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Directory.Exists(RootPath))
        {
            return Task.CompletedTask;
        }

        foreach (var directory in Directory.EnumerateDirectories(RootPath))
        {
            cancellationToken.ThrowIfCancellationRequested();
            TryDeleteDirectory(directory);
        }

        return Task.CompletedTask;
    }

    public Task CleanupJobAsync(string jobId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        TryDeleteDirectory(Path.Combine(RootPath, jobId));
        return Task.CompletedTask;
    }

    private static void TryDeleteDirectory(string directory)
    {
        try
        {
            Directory.Delete(directory, recursive: true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
