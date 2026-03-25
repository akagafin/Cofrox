using Cofrox.App.Services;
using Cofrox.App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Cofrox.App;

public sealed partial class ShellPage : Page
{
    private NavigationService? _navigationService;
    private ShellViewModel? _viewModel;

    public ShellViewModel ViewModel => _viewModel ??= App.GetService<ShellViewModel>();

    private NavigationService NavigationService => _navigationService ??= App.GetService<NavigationService>();

    public ShellPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        DataContext ??= ViewModel;
        NavigationService.Initialize(ContentFrame);

        if (NavigationService.CurrentPageType is null)
        {
            RootNavigationView.SelectedItem = RootNavigationView.MenuItems[0];
            NavigationService.Navigate(typeof(Pages.HomePage));
        }
    }

    private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer?.Tag is not string tag)
        {
            return;
        }

        ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ShellTransition", ContentFrame);

        var pageType = tag switch
        {
            "formats" => typeof(Pages.FormatsPage),
            "history" => typeof(Pages.HistoryPage),
            "settings" => typeof(Pages.SettingsPage),
            _ => typeof(Pages.HomePage),
        };

        ViewModel.PageTitle = args.SelectedItemContainer.Content?.ToString() ?? "Cofrox";
        NavigationService.Navigate(pageType);
    }
}
