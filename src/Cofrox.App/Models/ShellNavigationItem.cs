using Microsoft.UI.Xaml.Controls;

namespace Cofrox.App.Models;

public sealed record ShellNavigationItem(string Key, string Title, Symbol Icon, Type PageType);
