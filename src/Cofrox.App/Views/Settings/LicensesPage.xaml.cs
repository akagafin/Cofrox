using Cofrox.App.Services;
using Cofrox.App.ViewModels.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Cofrox.App.Views.Settings;

public sealed partial class LicensesPage : Page
{
    private readonly AppResourceService _resourceService = App.GetService<AppResourceService>();
    public LicensesViewModel ViewModel { get; } = App.GetService<LicensesViewModel>();

    public LicensesPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        BackButton.Label = _resourceService.GetString("LegalCommandBack");
        CopyButton.Label = _resourceService.GetString("LegalCommandCopyAllNotices");
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }
}
