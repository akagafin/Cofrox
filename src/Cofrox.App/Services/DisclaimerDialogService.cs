using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Cofrox.App.Services;

public sealed class DisclaimerDialogService(AppResourceService appResourceService)
{
    public async Task<bool> ShowFirstLaunchDialogAsync(FrameworkElement host)
    {
        var dialog = CreateDialog(host, allowDecline: true);
        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    public async Task ShowReadOnlyDialogAsync(FrameworkElement host)
    {
        var dialog = CreateDialog(host, allowDecline: false);
        await dialog.ShowAsync();
    }

    private ContentDialog CreateDialog(FrameworkElement host, bool allowDecline)
    {
        var contentHost = new StackPanel
        {
            Spacing = 8,
        };

        LegalDocumentRenderer.Render(contentHost, appResourceService.GetString("LegalDisclaimerMarkdown"));

        var dialog = new ContentDialog
        {
            XamlRoot = host.XamlRoot,
            Title = appResourceService.GetString("LegalDisclaimerDialogTitle"),
            Content = new ScrollViewer
            {
                MaxHeight = 320,
                Content = contentHost,
            },
            PrimaryButtonText = appResourceService.GetString(allowDecline ? "LegalDisclaimerPrimaryButton" : "LegalCloseButton"),
            SecondaryButtonText = allowDecline ? appResourceService.GetString("LegalDisclaimerSecondaryButton") : string.Empty,
            DefaultButton = ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = allowDecline,
        };

        if (allowDecline)
        {
            dialog.Closing += static (_, args) =>
            {
                if (args.Result == ContentDialogResult.None)
                {
                    args.Cancel = true;
                }
            };
        }

        return dialog;
    }
}
