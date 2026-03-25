using Cofrox.Core.Services;
using Cofrox.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Cofrox.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCofroxCore(this IServiceCollection services)
    {
        services.AddSingleton<IFormatCatalog, FormatCatalogService>();
        services.AddSingleton<FormatDetectionService>();
        return services;
    }
}
