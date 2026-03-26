using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cofrox.Core.Constants;
using Cofrox.Core.Utilities;
using Cofrox.Domain.Entities;
using Cofrox.Domain.Enums;
using Cofrox.Domain.Interfaces;
using Cofrox.Domain.ValueObjects;
using Microsoft.UI.Xaml.Controls;

namespace Cofrox.App.ViewModels;

public sealed partial class FileCardViewModel : ObservableObject
{
    private readonly FileItem _file;
    private readonly IFormatCatalog _formatCatalog;
    private readonly IConversionCoordinator _conversionCoordinator;
    private readonly ISettingsRepository _settingsRepository;
    private readonly ITempFileService _tempFileService;
    private CancellationTokenSource? _conversionCts;

    public FileCardViewModel(
        FileItem file,
        IFormatCatalog formatCatalog,
        IConversionCoordinator conversionCoordinator,
        ISettingsRepository settingsRepository,
        ITempFileService tempFileService)
    {
        _file = file;
        _formatCatalog = formatCatalog;
        _conversionCoordinator = conversionCoordinator;
        _settingsRepository = settingsRepository;
        _tempFileService = tempFileService;

        AvailableTargets = _formatCatalog.GetTargets(file.SourceExtension).ToArray();
        selectedTarget = AvailableTargets.FirstOrDefault();
        BuildOptions();
    }

    public event Action<FileCardViewModel>? RemoveRequested;

    public string FileName => _file.FileName;

    public string SourceExtension => _file.SourceExtension.ToUpperInvariant();

    public string SourceGlyph => _file.SourceFamily switch
    {
        FileFamily.Video => "\uE714",
        FileFamily.Audio => "\uE8D6",
        FileFamily.Image or FileFamily.RawImage => "\uE91B",
        FileFamily.Pdf or FileFamily.Document => "\uE8A5",
        FileFamily.Archive => "\uE7B8",
        FileFamily.Data or FileFamily.Spreadsheet => "\uE9D2",
        _ => "\uE8A5",
    };

    public string FileSize => FileSizeFormatter.Format(_file.FileSizeBytes);

    public IReadOnlyList<FormatDefinition> AvailableTargets { get; }

    public ObservableCollection<OptionItemViewModel> Options { get; } = [];

    public bool HasOptions => Options.Any(static option => option.IsVisible);

    public bool IsUnsupported => AvailableTargets.Count == 0;

    [ObservableProperty]
    private FormatDefinition? selectedTarget;

    [ObservableProperty]
    private double progress;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusText = string.Empty;

    [ObservableProperty]
    private bool isInfoBarOpen;

    [ObservableProperty]
    private string infoMessage = string.Empty;

    [ObservableProperty]
    private InfoBarSeverity infoSeverity = InfoBarSeverity.Informational;

    partial void OnSelectedTargetChanged(FormatDefinition? value)
    {
        BuildOptions();
    }

    [RelayCommand]
    private async Task ConvertAsync()
    {
        if (SelectedTarget is null || IsBusy)
        {
            return;
        }

        IsBusy = true;
        Progress = 0;
        StatusText = AppCopy.Converting;
        IsInfoBarOpen = false;
        _conversionCts = new CancellationTokenSource();

        try
        {
            var settings = await _settingsRepository.LoadAsync(_conversionCts.Token);
            Directory.CreateDirectory(settings.OutputFolderPath);

            var outputPath = Path.Combine(
                settings.OutputFolderPath,
                $"{Path.GetFileNameWithoutExtension(_file.FileName)}.{SelectedTarget.Extension}");

            var job = new ConversionJob
            {
                Id = Guid.NewGuid().ToString("N"),
                SourceFile = _file,
                TargetExtension = SelectedTarget.Extension,
                OutputPath = outputPath,
                Options = Options.Where(static option => option.IsVisible)
                    .ToDictionary(static option => option.Key, static option => option.GetValue()),
            };

            _tempFileService.CreateJobFolder(job.Id);
            var result = await _conversionCoordinator.ConvertAsync(
                job,
                new Progress<double>(value => Progress = value),
                _conversionCts.Token);

            Progress = result.Status == ConversionStatus.Completed ? 1 : Progress;
            StatusText = result.Status switch
            {
                ConversionStatus.Completed => AppCopy.Done,
                ConversionStatus.Warning => result.Message ?? AppCopy.Unsupported,
                ConversionStatus.Failed => result.Message ?? AppCopy.Error,
                _ => result.Message ?? AppCopy.Error,
            };

            InfoSeverity = result.Status switch
            {
                ConversionStatus.Completed => InfoBarSeverity.Success,
                ConversionStatus.Warning => InfoBarSeverity.Warning,
                _ => InfoBarSeverity.Error,
            };
            InfoMessage = result.Message ?? StatusText;
            IsInfoBarOpen = result.Status != ConversionStatus.Completed;
        }
        catch (OperationCanceledException)
        {
            StatusText = "Conversion cancelled.";
            InfoSeverity = InfoBarSeverity.Warning;
            InfoMessage = "The current conversion was cancelled.";
            IsInfoBarOpen = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _conversionCts?.Cancel();
    }

    [RelayCommand]
    private void ResetOptions()
    {
        foreach (var option in Options)
        {
            option.Reset();
        }

        RefreshOptionVisibility();
    }

    [RelayCommand]
    private void Remove()
    {
        RemoveRequested?.Invoke(this);
    }

    private void BuildOptions()
    {
        Options.Clear();
        if (SelectedTarget is null)
        {
            OnPropertyChanged(nameof(HasOptions));
            return;
        }

        foreach (var definition in _formatCatalog.GetOptions(_file.SourceExtension, SelectedTarget.Extension))
        {
            var option = new OptionItemViewModel(definition);
            option.PropertyChanged += OnOptionPropertyChanged;
            Options.Add(option);
        }

        RefreshOptionVisibility();
        OnPropertyChanged(nameof(HasOptions));
    }

    private void RefreshOptionVisibility()
    {
        var targetExtension = SelectedTarget?.Extension ?? string.Empty;
        var isLosslessAudio = targetExtension is "flac" or "wav";
        var qualityMode = Options.FirstOrDefault(static option => option.Key == "quality_mode")?.SelectedChoiceKey ?? "crf";
        var audioMode = Options.FirstOrDefault(static option => option.Key == "audio_mode")?.SelectedChoiceKey ?? "keep";
        var resizeMode = Options.FirstOrDefault(static option => option.Key == "resize_mode")?.SelectedChoiceKey ?? "none";
        var videoPreset = Options.FirstOrDefault(static option => option.Key == "video_preset")?.SelectedChoiceKey ?? "custom";
        var videoEncoder = Options.FirstOrDefault(static option => option.Key == "video_encoder")?.SelectedChoiceKey ?? "software";
        var isVideoTarget = SelectedTarget?.Family is FileFamily.Video;
        var presetCustom = string.Equals(videoPreset, "custom", StringComparison.OrdinalIgnoreCase);

        foreach (var option in Options)
        {
            option.IsVisible = option.Key switch
            {
                "target_bitrate" => qualityMode == "bitrate" && presetCustom && isVideoTarget,
                "quality_value" => qualityMode != "bitrate" && presetCustom && isVideoTarget,
                "resolution" or "video_codec" or "quality_mode" or "frame_rate" => presetCustom && isVideoTarget,
                "encoding_speed" => presetCustom && isVideoTarget && string.Equals(videoEncoder, "software", StringComparison.OrdinalIgnoreCase),
                "video_encoder" => isVideoTarget,
                "audio_codec" or "audio_bitrate" => audioMode == "reencode",
                "bitrate" => !isLosslessAudio,
                "width" or "height" or "resample_filter" => resizeMode != "none",
                "background" => targetExtension is "jpg" or "bmp",
                "sfx" => targetExtension == "7z",
                _ => true,
            };
        }

        OnPropertyChanged(nameof(HasOptions));
    }

    private void OnOptionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OptionItemViewModel.IsVisible))
        {
            return;
        }

        RefreshOptionVisibility();
    }
}
