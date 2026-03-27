using Cofrox.App.Services;
using Cofrox.Application;
using Cofrox.Converters;
using Cofrox.Core;
using Cofrox.Data;
using Cofrox.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;

namespace Cofrox.App;

public partial class App : Application
{
    private static IHost? _host;

    public static IHost Host => _host ??= CreateHostBuilder().Build();

    public static T GetService<T>() where T : notnull => Host.Services.GetRequiredService<T>();

    public App()
    {
        InitializeComponent();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        await Host.StartAsync();

        var tempFileService = GetService<ITempFileService>();
        await tempFileService.CleanupStaleFilesAsync(CancellationToken.None);

        var window = GetService<MainWindow>();
        GetService<WindowService>().Initialize(window);
        if (!await EnsureDisclaimerAcceptedAsync(window))
        {
            Current.Exit();
            return;
        }

        window.EnsureShellContent();
        await GetService<ThemeService>().InitializeAsync(CancellationToken.None);
        window.Activate();
    }

    private static IHostBuilder CreateHostBuilder() =>
        Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices(static services =>
            {
                services.AddCofroxCore();
                services.AddCofroxData();
                services.AddCofroxApplication();
                services.AddCofroxConverters();
                services.AddCofroxApp();
            });

    private static async Task<bool> EnsureDisclaimerAcceptedAsync(MainWindow window)
    {
        var localSettings = ApplicationData.Current.LocalSettings;
        if (localSettings.Values["disclaimer_accepted_v1"] is true)
        {
            return true;
        }

        var disclaimerWindow = new Window
        {
            Content = new Grid(),
        };

        disclaimerWindow.Activate();
        var host = (FrameworkElement)disclaimerWindow.Content;

        try
        {
            var accepted = await GetService<DisclaimerDialogService>().ShowFirstLaunchDialogAsync(host);
            if (accepted)
            {
                localSettings.Values["disclaimer_accepted_v1"] = true;
            }

            return accepted;
        }
        finally
        {
            disclaimerWindow.Close();
        }
    }
}
