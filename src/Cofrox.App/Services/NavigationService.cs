using Microsoft.UI.Xaml.Controls;

namespace Cofrox.App.Services;

public sealed class NavigationService
{
    private Frame? _frame;

    public Type? CurrentPageType => _frame?.CurrentSourcePageType;

    public void Initialize(Frame frame) => _frame = frame;

    public bool Navigate(Type pageType, object? parameter = null) =>
        _frame?.Navigate(pageType, parameter) ?? false;
}
