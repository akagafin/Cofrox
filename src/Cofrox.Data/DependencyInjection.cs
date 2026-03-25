using Cofrox.Data.Repositories;
using Cofrox.Data.Services;
using Cofrox.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Cofrox.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddCofroxData(this IServiceCollection services)
    {
        services.AddSingleton<ISettingsRepository, ApplicationDataSettingsRepository>();
        services.AddSingleton<IHistoryRepository, SqliteHistoryRepository>();
        services.AddSingleton<ITempFileService, TempFileService>();
        services.AddSingleton<ISystemProfileService, SystemProfileService>();
        services.AddSingleton<IExternalToolLocator, BundledToolLocator>();
        return services;
    }
}
