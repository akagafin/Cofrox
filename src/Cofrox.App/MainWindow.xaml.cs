using Cofrox.App.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Cofrox.App;

public sealed partial class MainWindow : Window
{
    private bool _contentInitialized;

    public MainWindow()
    {
        InitializeComponent();
        SystemBackdrop = new MicaBackdrop();
    }

    public void EnsureShellContent()
    {
        if (_contentInitialized)
        {
            return;
        }

        Content = App.GetService<ShellPage>();
        _contentInitialized = true;
    }
}
