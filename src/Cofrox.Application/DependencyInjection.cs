using Cofrox.Application.Interfaces;
using Cofrox.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cofrox.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCofroxApplication(this IServiceCollection services)
    {
        services.AddSingleton<IPresetManager, PresetManager>();
        services.AddSingleton<ISmartConversionAdvisor, SmartConversionAdvisor>();
        services.AddSingleton<IFFmpegCommandBuilder, FFmpegCommandBuilder>();
        services.AddSingleton<IQueueManager, QueueManager>();
        return services;
    }
}
