using Cofrox.Domain.Entities;
using Cofrox.Domain.Enums;
using Cofrox.Domain.Interfaces;
using Windows.Storage;

namespace Cofrox.Data.Repositories;

public sealed class ApplicationDataSettingsRepository : ISettingsRepository
{
    private readonly ApplicationDataContainer _settings = ApplicationData.Current.LocalSettings;

    public Task<AppSettings> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var defaults = AppSettings.Default;
        var result = new AppSettings
        {
            OutputFolderPath = GetString("OutputFolderPath") ?? defaults.OutputFolderPath,
            DefaultVideoQuality = GetString("DefaultVideoQuality") ?? defaults.DefaultVideoQuality,
            DefaultImageQuality = GetInt32("DefaultImageQuality") ?? defaults.DefaultImageQuality,
            MaxParallelConversions = GetInt32("MaxParallelConversions") ?? defaults.MaxParallelConversions,
            AutoDeleteTempFiles = GetBoolean("AutoDeleteTempFiles") ?? defaults.AutoDeleteTempFiles,
            ThemeMode = Enum.TryParse<AppThemeMode>(GetString("ThemeMode"), true, out var themeMode) ? themeMode : defaults.ThemeMode,
            SaveHistory = GetBoolean("SaveHistory") ?? defaults.SaveHistory,
            LibreOfficePath = GetString("LibreOfficePath"),
        };

        return Task.FromResult(result);
    }

    public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _settings.Values["OutputFolderPath"] = settings.OutputFolderPath;
        _settings.Values["DefaultVideoQuality"] = settings.DefaultVideoQuality;
        _settings.Values["DefaultImageQuality"] = settings.DefaultImageQuality;
        _settings.Values["MaxParallelConversions"] = settings.MaxParallelConversions;
        _settings.Values["AutoDeleteTempFiles"] = settings.AutoDeleteTempFiles;
        _settings.Values["ThemeMode"] = settings.ThemeMode.ToString();
        _settings.Values["SaveHistory"] = settings.SaveHistory;
        _settings.Values["LibreOfficePath"] = settings.LibreOfficePath ?? string.Empty;
        return Task.CompletedTask;
    }

    private string? GetString(string key) =>
        _settings.Values.TryGetValue(key, out var value) ? value?.ToString() : null;

    private int? GetInt32(string key) =>
        _settings.Values.TryGetValue(key, out var value) && value is int intValue ? intValue : null;

    private bool? GetBoolean(string key) =>
        _settings.Values.TryGetValue(key, out var value) && value is bool boolValue ? boolValue : null;
}
