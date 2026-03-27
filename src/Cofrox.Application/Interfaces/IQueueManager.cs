using Cofrox.Application.Models;
using Cofrox.Domain.Entities;
using Cofrox.Domain.Enums;

namespace Cofrox.Application.Interfaces;

public interface IQueueManager
{
    Task<IReadOnlyList<QueueItemState>> LoadSnapshotAsync(CancellationToken cancellationToken);

    Task EnqueueAsync(ConversionJob job, CancellationToken cancellationToken);

    Task UpdateStatusAsync(string jobId, ConversionStatus status, string? lastError, CancellationToken cancellationToken);

    Task PauseAsync(string jobId, CancellationToken cancellationToken);

    Task ResumeAsync(string jobId, CancellationToken cancellationToken);

    Task RetryFailedAsync(string jobId, CancellationToken cancellationToken);

    Task RemoveAsync(string jobId, CancellationToken cancellationToken);
}
