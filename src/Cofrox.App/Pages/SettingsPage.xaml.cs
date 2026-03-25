using Cofrox.App.Services;
using Cofrox.App.Views.Settings;
using Cofrox.App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Cofrox.App.Pages;

public sealed partial class SettingsPage : Page
{
    private SettingsViewModel? _viewModel;
    private readonly AppResourceService _resourceService = App.GetService<AppResourceService>();

    public SettingsViewModel ViewModel => _viewModel ??= App.GetService<SettingsViewModel>();

    public SettingsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        DataContext ??= ViewModel;
        LegalExpander.Header = _resourceService.GetString("LegalSettingsHeader");
        PrivacyPolicyText.Text = _resourceService.GetString("LegalSettingsPrivacyPolicy");
        TermsOfUseText.Text = _resourceService.GetString("LegalSettingsTermsOfUse");
        LicensesText.Text = _resourceService.GetString("LegalSettingsOpenSourceLicenses");
        DisclaimerText.Text = _resourceService.GetString("LegalSettingsDisclaimer");
        await ViewModel.LoadAsync();
    }

    private void PrivacyPolicyButton_Click(object sender, RoutedEventArgs e)
    {
        App.GetService<NavigationService>().Navigate(typeof(PrivacyPolicyPage));
    }

    private void TermsOfUseButton_Click(object sender, RoutedEventArgs e)
    {
        App.GetService<NavigationService>().Navigate(typeof(TermsOfUsePage));
    }

    private void LicensesButton_Click(object sender, RoutedEventArgs e)
    {
        App.GetService<NavigationService>().Navigate(typeof(LicensesPage));
    }

    private async void DisclaimerButton_Click(object sender, RoutedEventArgs e)
    {
        await App.GetService<DisclaimerDialogService>().ShowReadOnlyDialogAsync(this);
    }
}
