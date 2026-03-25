using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cofrox.App.Services;
using Cofrox.Core.Constants;
using Cofrox.Core.Services;
using Cofrox.Domain.Entities;
using Cofrox.Domain.Enums;
using Cofrox.Domain.Interfaces;
using Microsoft.UI.Xaml.Controls;

namespace Cofrox.App.ViewModels;

public sealed partial class HomeViewModel(
    FilePickerService filePickerService,
    FormatDetectionService formatDetectionService,
    IFormatCatalog formatCatalog,
    IConversionCoordinator conversionCoordinator,
    ISettingsRepository settingsRepository,
    ITempFileService tempFileService,
    ISystemProfileService systemProfileService) : ObservableObject
{
    public ObservableCollection<FileCardViewModel> QueueItems { get; } = [];

    [ObservableProperty]
    private bool isDropOver;

    [ObservableProperty]
    private bool isNoticeOpen;

    [ObservableProperty]
    private string noticeMessage = AppCopy.EmptyState;

    [ObservableProperty]
    private InfoBarSeverity noticeSeverity = InfoBarSeverity.Informational;

    [RelayCommand]
    private async Task PickFilesAsync()
    {
        var files = await filePickerService.PickFilesAsync();
        await ImportFilesAsync(files);
    }

    [RelayCommand]
    private async Task ConvertAllAsync()
    {
        await Task.WhenAll(QueueItems.Select(item => item.ConvertCommand.ExecuteAsync(null)));
    }

    [RelayCommand]
    private void ClearAll()
    {
        QueueItems.Clear();
        NoticeMessage = AppCopy.EmptyState;
        NoticeSeverity = InfoBarSeverity.Informational;
        IsNoticeOpen = false;
    }

    public async Task ImportFilesAsync(IEnumerable<string> paths)
    {
        var profile = systemProfileService.GetCurrent();
        foreach (var path in paths.Where(File.Exists))
        {
            var definition = formatDetectionService.Detect(path);
            var file = new FileInfo(path);
            var fileItem = new FileItem
            {
                Id = Guid.NewGuid().ToString("N"),
                FileName = file.Name,
                SourcePath = file.FullName,
                SourceExtension = definition.Extension,
                SourceFamily = definition.Family,
                FileSizeBytes = file.Length,
            };

            var viewModel = new FileCardViewModel(fileItem, formatCatalog, conversionCoordinator, settingsRepository, tempFileService);
            viewModel.RemoveRequested += RemoveQueueItem;
            QueueItems.Add(viewModel);

            if (profile.IsLowMemoryDevice && file.Length > 2L * 1024 * 1024 * 1024)
            {
                NoticeSeverity = InfoBarSeverity.Warning;
                NoticeMessage = AppCopy.LargeFile;
                IsNoticeOpen = true;
            }
        }

        if (QueueItems.Count > 0 && !IsNoticeOpen)
        {
            NoticeSeverity = InfoBarSeverity.Success;
            NoticeMessage = $"{QueueItems.Count} file(s) ready to convert.";
            IsNoticeOpen = true;
        }
    }

    private void RemoveQueueItem(FileCardViewModel item)
    {
        item.RemoveRequested -= RemoveQueueItem;
        QueueItems.Remove(item);
    }
}
