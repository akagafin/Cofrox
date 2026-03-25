using Cofrox.App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Cofrox.App.Pages;

public sealed partial class HistoryPage : Page
{
    private HistoryViewModel? _viewModel;

    public HistoryViewModel ViewModel => _viewModel ??= App.GetService<HistoryViewModel>();

    public HistoryPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        DataContext ??= ViewModel;
        await ViewModel.LoadAsync();
    }
}
