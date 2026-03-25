using Cofrox.App.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Cofrox.App.Views.Settings;

public sealed partial class TermsOfUsePage : Page
{
    private readonly AppResourceService _resourceService = App.GetService<AppResourceService>();
    private readonly ClipboardService _clipboardService = App.GetService<ClipboardService>();

    public TermsOfUsePage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        BackButton.Label = _resourceService.GetString("LegalCommandBack");
        CopyButton.Label = _resourceService.GetString("LegalCommandCopyText");
        LegalDocumentRenderer.Render(DocumentHost, _resourceService.GetString("LegalTermsOfUseMarkdown"));
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        _clipboardService.CopyText(_resourceService.GetString("LegalTermsOfUseMarkdown"));
    }
}
