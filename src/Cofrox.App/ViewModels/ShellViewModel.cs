using Cofrox.App.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace Cofrox.App.ViewModels;

public sealed partial class ShellViewModel : ObservableObject
{
    public IReadOnlyList<ShellNavigationItem> Items { get; } =
    [
        new("home", "Home", Symbol.Home, typeof(Pages.HomePage)),
        new("formats", "Formats", Symbol.AllApps, typeof(Pages.FormatsPage)),
        new("history", "History", Symbol.Clock, typeof(Pages.HistoryPage)),
        new("settings", "Settings", Symbol.Setting, typeof(Pages.SettingsPage)),
    ];

    [ObservableProperty]
    private string pageTitle = "Cofrox";
}
