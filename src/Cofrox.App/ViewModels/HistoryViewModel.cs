using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cofrox.App.Services;
using Cofrox.Core.Constants;
using Cofrox.Domain.Entities;
using Cofrox.Domain.Interfaces;

namespace Cofrox.App.ViewModels;

public sealed partial class HistoryViewModel(
    IHistoryRepository historyRepository,
    DialogService dialogService) : ObservableObject
{
    public ObservableCollection<HistoryEntry> Entries { get; } = [];

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string emptyMessage = AppCopy.EmptyHistory;

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;
        Entries.Clear();
        try
        {
            var entries = await historyRepository.GetRecentAsync(50, CancellationToken.None);
            foreach (var entry in entries)
            {
                Entries.Add(entry);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ClearAsync()
    {
        var confirmed = await dialogService.ConfirmAsync("Clear History", AppCopy.ClearHistoryConfirmation, "Delete");
        if (!confirmed)
        {
            return;
        }

        await historyRepository.ClearAsync(CancellationToken.None);
        await LoadAsync();
    }
}
