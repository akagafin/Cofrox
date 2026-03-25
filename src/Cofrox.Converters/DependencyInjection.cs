using Cofrox.Converters.Engines;
using Cofrox.Converters.Factories;
using Cofrox.Converters.Infrastructure;
using Cofrox.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Cofrox.Converters;

public static class DependencyInjection
{
    public static IServiceCollection AddCofroxConverters(this IServiceCollection services)
    {
        services.AddSingleton<IExternalProcessRunner, ExternalProcessRunner>();
        services.AddSingleton<IConversionEngine, MultimediaConversionEngine>();
        services.AddSingleton<IConversionEngine, ImageConversionEngine>();
        services.AddSingleton<IConversionEngine, DocumentConversionEngine>();
        services.AddSingleton<IConversionEngine, ArchiveConversionEngine>();
        services.AddSingleton<IConversionEngine, DataConversionEngine>();
        services.AddSingleton<IConversionEngine, Model3DConversionEngine>();
        services.AddSingleton<IConversionEngine, SubtitleConversionEngine>();
        services.AddSingleton<IConversionEngine, FontConversionEngine>();
        services.AddSingleton<IConversionEngine, UnsupportedConversionEngine>();
        services.AddSingleton<IConversionCoordinator, ConversionCoordinator>();
        return services;
    }
}
