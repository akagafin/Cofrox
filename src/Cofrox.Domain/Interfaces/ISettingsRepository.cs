using Cofrox.Domain.Entities;

namespace Cofrox.Domain.Interfaces;

public interface ISettingsRepository
{
    Task<AppSettings> LoadAsync(CancellationToken cancellationToken);

    Task SaveAsync(AppSettings settings, CancellationToken cancellationToken);
}
