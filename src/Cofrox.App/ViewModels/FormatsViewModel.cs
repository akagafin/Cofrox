using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Cofrox.Domain.Interfaces;
using Cofrox.Domain.ValueObjects;

namespace Cofrox.App.ViewModels;

public sealed partial class FormatsViewModel : ObservableObject
{
    private readonly IReadOnlyList<FormatDefinition> _allFormats;

    public FormatsViewModel(IFormatCatalog formatCatalog)
    {
        _allFormats = formatCatalog.GetAllFormats();
        Categories = ["All", .. _allFormats.Select(static item => item.Category).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(static item => item)];
        ApplyFilter();
    }

    public ObservableCollection<FormatDefinition> VisibleFormats { get; } = [];

    public IReadOnlyList<string> Categories { get; }

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string selectedCategory = "All";

    [ObservableProperty]
    private bool isTeachingTipOpen = true;

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnSelectedCategoryChanged(string value) => ApplyFilter();

    public void ApplyFilter()
    {
        var filtered = _allFormats
            .Where(item => SelectedCategory == "All" || string.Equals(item.Category, SelectedCategory, StringComparison.OrdinalIgnoreCase))
            .Where(item =>
                string.IsNullOrWhiteSpace(SearchText) ||
                item.Extension.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                item.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                item.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            .OrderBy(static item => item.Category)
            .ThenBy(static item => item.DisplayName)
            .ToArray();

        VisibleFormats.Clear();
        foreach (var item in filtered)
        {
            VisibleFormats.Add(item);
        }
    }
}
