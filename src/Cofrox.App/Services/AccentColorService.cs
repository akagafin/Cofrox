using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace Cofrox.App.Services;

public sealed class AccentColorService
{
    private readonly UISettings _uiSettings = new();
    private DispatcherQueue? _dispatcherQueue;

    public Color CurrentAccentColor { get; private set; } = ColorHelper.FromArgb(255, 0, 180, 216);

    public void Initialize(DispatcherQueue dispatcherQueue)
    {
        _dispatcherQueue = dispatcherQueue;
        _uiSettings.ColorValuesChanged -= OnColorValuesChanged;
        _uiSettings.ColorValuesChanged += OnColorValuesChanged;
        UpdateAccentResources();
    }

    private void OnColorValuesChanged(UISettings sender, object args)
    {
        _dispatcherQueue?.TryEnqueue(UpdateAccentResources);
    }

    private void UpdateAccentResources()
    {
        try
        {
            CurrentAccentColor = _uiSettings.GetColorValue(UIColorType.Accent);
        }
        catch
        {
            CurrentAccentColor = ColorHelper.FromArgb(255, 0, 180, 216);
        }

        Application.Current.Resources["CofroxAccentColor"] = CurrentAccentColor;
        Application.Current.Resources["CofroxAccentBrush"] = new SolidColorBrush(CurrentAccentColor);
    }
}
