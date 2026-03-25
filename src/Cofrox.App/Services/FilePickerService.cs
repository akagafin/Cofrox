using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Cofrox.App.Services;

public sealed class FilePickerService(WindowService windowService)
{
    public async Task<IReadOnlyList<string>> PickFilesAsync()
    {
        var picker = new FileOpenPicker();
        InitializeWithWindow.Initialize(picker, windowService.Hwnd);
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.ViewMode = PickerViewMode.Thumbnail;
        picker.FileTypeFilter.Add("*");

        var files = await picker.PickMultipleFilesAsync();
        return files?.Select(static file => file.Path).Where(static path => !string.IsNullOrWhiteSpace(path)).ToArray() ?? [];
    }

    public async Task<string?> PickFolderAsync()
    {
        var picker = new FolderPicker();
        InitializeWithWindow.Initialize(picker, windowService.Hwnd);
        picker.SuggestedStartLocation = PickerLocationId.Downloads;
        picker.FileTypeFilter.Add("*");

        var folder = await picker.PickSingleFolderAsync();
        return folder?.Path;
    }
}
