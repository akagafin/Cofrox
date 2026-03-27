using Cofrox.App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel.DataTransfer;

namespace Cofrox.App.Pages;

public sealed partial class HomePage : Page
{
    private HomeViewModel? _viewModel;

    public HomeViewModel ViewModel => _viewModel ??= App.GetService<HomeViewModel>();

    public HomePage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        DataContext ??= ViewModel;
    }

    private void DropZone_DragEnter(object sender, DragEventArgs e)
    {
        ViewModel.IsDropOver = true;
        DropOutline.Stroke = (Brush)Microsoft.UI.Xaml.Application.Current.Resources["CofroxAccentBrush"];
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private void DropZone_DragLeave(object sender, DragEventArgs e)
    {
        ViewModel.IsDropOver = false;
        DropOutline.Stroke = (Brush)Microsoft.UI.Xaml.Application.Current.Resources["CofroxMutedOutlineBrush"];
    }

    private void DropZone_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private async void DropZone_Drop(object sender, DragEventArgs e)
    {
        ViewModel.IsDropOver = false;
        DropOutline.Stroke = (Brush)Microsoft.UI.Xaml.Application.Current.Resources["CofroxMutedOutlineBrush"];

        if (!e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            return;
        }

        var items = await e.DataView.GetStorageItemsAsync();
        await ViewModel.ImportFilesAsync(items.Select(static item => item.Path));
    }
}
