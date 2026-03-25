using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace Cofrox.App.Services;

public sealed class WindowService
{
    public Window? Window { get; private set; }

    public nint Hwnd => Window is null ? 0 : WindowNative.GetWindowHandle(Window);

    public void Initialize(Window window) => Window = window;
}
