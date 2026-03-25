using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cofrox.App.Services;
using Cofrox.Domain.Entities;
using Cofrox.Domain.Enums;
using Cofrox.Domain.Interfaces;

namespace Cofrox.App.ViewModels;

public sealed partial class SettingsViewModel(
    FilePickerService filePickerService,
    ThemeService themeService,
    ISettingsRepository settingsRepository,
    ITempFileService tempFileService) : ObservableObject
{
    [ObservableProperty]
    private string outputFolderPath = string.Empty;

    [ObservableProperty]
    private string defaultVideoQuality = "High";

    [ObservableProperty]
    private double defaultImageQuality = 85;

    [ObservableProperty]
    private int maxParallelConversions;

    [ObservableProperty]
    private bool autoDeleteTempFiles = true;

    [ObservableProperty]
    private bool saveHistory = true;

    [ObservableProperty]
    private AppThemeMode themeMode = AppThemeMode.System;

    [ObservableProperty]
    private string libreOfficePath = string.Empty;

    [ObservableProperty]
    private bool isLoaded;

    public string ThemeModeName
    {
        get => ThemeMode.ToString();
        set
        {
            if (Enum.TryParse<AppThemeMode>(value, true, out var parsed))
            {
                ThemeMode = parsed;
            }
        }
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsLoaded)
        {
            return;
        }

        var settings = await settingsRepository.LoadAsync(CancellationToken.None);
        OutputFolderPath = settings.OutputFolderPath;
        DefaultVideoQuality = settings.DefaultVideoQuality;
        DefaultImageQuality = settings.DefaultImageQuality;
        MaxParallelConversions = settings.MaxParallelConversions;
        AutoDeleteTempFiles = settings.AutoDeleteTempFiles;
        SaveHistory = settings.SaveHistory;
        ThemeMode = settings.ThemeMode;
        OnPropertyChanged(nameof(ThemeModeName));
        LibreOfficePath = settings.LibreOfficePath ?? string.Empty;
        IsLoaded = true;
    }

    [RelayCommand]
    private async Task PickOutputFolderAsync()
    {
        var folder = await filePickerService.PickFolderAsync();
        if (!string.IsNullOrWhiteSpace(folder))
        {
            OutputFolderPath = folder;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var settings = new AppSettings
        {
            OutputFolderPath = OutputFolderPath,
            DefaultVideoQuality = DefaultVideoQuality,
            DefaultImageQuality = (int)DefaultImageQuality,
            MaxParallelConversions = MaxParallelConversions,
            AutoDeleteTempFiles = AutoDeleteTempFiles,
            SaveHistory = SaveHistory,
            ThemeMode = ThemeMode,
            LibreOfficePath = string.IsNullOrWhiteSpace(LibreOfficePath) ? null : LibreOfficePath,
        };

        await settingsRepository.SaveAsync(settings, CancellationToken.None);
        await themeService.SetThemeModeAsync(ThemeMode, CancellationToken.None);
    }

    partial void OnThemeModeChanged(AppThemeMode value) => OnPropertyChanged(nameof(ThemeModeName));

    [RelayCommand]
    private async Task ClearTempAsync()
    {
        await tempFileService.CleanupStaleFilesAsync(CancellationToken.None);
    }
}
