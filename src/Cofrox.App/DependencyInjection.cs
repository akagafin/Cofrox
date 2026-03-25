using Cofrox.App.Pages;
using Cofrox.App.Services;
using Cofrox.App.ViewModels;
using Cofrox.App.ViewModels.Settings;
using Cofrox.App.Views.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Cofrox.App;

public static class DependencyInjection
{
    public static IServiceCollection AddCofroxApp(this IServiceCollection services)
    {
        services.AddSingleton<WindowService>();
        services.AddSingleton<FilePickerService>();
        services.AddSingleton<DialogService>();
        services.AddSingleton<NavigationService>();
        services.AddSingleton<AccentColorService>();
        services.AddSingleton<AppResourceService>();
        services.AddSingleton<ClipboardService>();
        services.AddSingleton<DisclaimerDialogService>();
        services.AddSingleton<ThemeService>();

        services.AddSingleton<ShellViewModel>();
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<FormatsViewModel>();
        services.AddSingleton<HistoryViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<LicensesViewModel>();

        services.AddSingleton<MainWindow>();
        services.AddSingleton<ShellPage>();
        services.AddSingleton<HomePage>();
        services.AddSingleton<FormatsPage>();
        services.AddSingleton<HistoryPage>();
        services.AddSingleton<SettingsPage>();
        services.AddSingleton<PrivacyPolicyPage>();
        services.AddSingleton<TermsOfUsePage>();
        services.AddSingleton<LicensesPage>();
        return services;
    }
}
