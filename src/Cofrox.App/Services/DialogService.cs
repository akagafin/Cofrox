using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Cofrox.App.Services;

public sealed class DialogService(WindowService windowService)
{
    public async Task<bool> ConfirmAsync(string title, string content, string primaryButtonText = "Confirm", string closeButtonText = "Cancel")
    {
        if (windowService.Window?.Content is not FrameworkElement frameworkElement)
        {
            return false;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = frameworkElement.XamlRoot,
            Title = title,
            Content = content,
            PrimaryButtonText = primaryButtonText,
            CloseButtonText = closeButtonText,
            DefaultButton = ContentDialogButton.Close,
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }
}
