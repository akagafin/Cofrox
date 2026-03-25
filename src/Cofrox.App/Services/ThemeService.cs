using Cofrox.Domain.Entities;
using Cofrox.Domain.Enums;
using Cofrox.Domain.Interfaces;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Windows.UI.ViewManagement;

namespace Cofrox.App.Services;

public sealed class ThemeService(
    WindowService windowService,
    AccentColorService accentColorService,
    ISettingsRepository settingsRepository)
{
    private readonly UISettings _uiSettings = new();
    private DispatcherQueue? _dispatcherQueue;
    private AppThemeMode _currentMode = AppThemeMode.System;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        accentColorService.Initialize(_dispatcherQueue);
        _uiSettings.ColorValuesChanged -= OnColorValuesChanged;
        _uiSettings.ColorValuesChanged += OnColorValuesChanged;

        var settings = await settingsRepository.LoadAsync(cancellationToken);
        _currentMode = settings.ThemeMode;
        ApplyTheme();
    }

    public async Task SetThemeModeAsync(AppThemeMode mode, CancellationToken cancellationToken)
    {
        _currentMode = mode;
        var settings = await settingsRepository.LoadAsync(cancellationToken);
        await settingsRepository.SaveAsync(
            new AppSettings
            {
                OutputFolderPath = settings.OutputFolderPath,
                DefaultVideoQuality = settings.DefaultVideoQuality,
                DefaultImageQuality = settings.DefaultImageQuality,
                MaxParallelConversions = settings.MaxParallelConversions,
                AutoDeleteTempFiles = settings.AutoDeleteTempFiles,
                ThemeMode = mode,
                SaveHistory = settings.SaveHistory,
                LibreOfficePath = settings.LibreOfficePath,
            },
            cancellationToken);
        _dispatcherQueue?.TryEnqueue(ApplyTheme);
    }

    private void OnColorValuesChanged(UISettings sender, object args)
    {
        if (_currentMode == AppThemeMode.System)
        {
            _dispatcherQueue?.TryEnqueue(ApplyTheme);
        }
    }

    private void ApplyTheme()
    {
        if (windowService.Window?.Content is not FrameworkElement frameworkElement)
        {
            return;
        }

        frameworkElement.RequestedTheme = _currentMode switch
        {
            AppThemeMode.Light => ElementTheme.Light,
            AppThemeMode.Dark => ElementTheme.Dark,
            _ => ElementTheme.Default,
        };
    }
}
