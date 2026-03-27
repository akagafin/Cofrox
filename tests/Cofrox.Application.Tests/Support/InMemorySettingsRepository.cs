using Cofrox.Domain.Entities;
using Cofrox.Domain.Interfaces;

namespace Cofrox.Application.Tests.Support;

internal sealed class InMemorySettingsRepository : ISettingsRepository
{
    private AppSettings _settings = AppSettings.Default;

    public Task<AppSettings> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_settings);
    }

    public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _settings = settings;
        return Task.CompletedTask;
    }
}
