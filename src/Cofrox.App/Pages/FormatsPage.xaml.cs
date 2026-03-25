using Cofrox.App.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Cofrox.App.Pages;

public sealed partial class FormatsPage : Page
{
    private FormatsViewModel? _viewModel;

    public FormatsViewModel ViewModel => _viewModel ??= App.GetService<FormatsViewModel>();

    public FormatsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        DataContext ??= ViewModel;
    }
}
