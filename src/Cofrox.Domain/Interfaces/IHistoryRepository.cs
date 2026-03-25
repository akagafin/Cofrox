using Cofrox.Domain.Entities;

namespace Cofrox.Domain.Interfaces;

public interface IHistoryRepository
{
    Task InitializeAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<HistoryEntry>> GetRecentAsync(int count, CancellationToken cancellationToken);

    Task AddAsync(HistoryEntry entry, CancellationToken cancellationToken);

    Task ClearAsync(CancellationToken cancellationToken);
}
