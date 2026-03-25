namespace Cofrox.Domain.Interfaces;

public interface ITempFileService
{
    string RootPath { get; }

    string CreateJobFolder(string jobId);

    Task CleanupStaleFilesAsync(CancellationToken cancellationToken);

    Task CleanupJobAsync(string jobId, CancellationToken cancellationToken);
}
