using CommunityToolkit.Mvvm.ComponentModel;
using Cofrox.Domain.Enums;
using Cofrox.Domain.ValueObjects;

namespace Cofrox.App.ViewModels;

public sealed partial class OptionItemViewModel : ObservableObject
{
    private readonly object? _defaultValue;

    public OptionItemViewModel(FormatOptionDefinition definition)
    {
        Key = definition.Key;
        DisplayName = definition.DisplayName;
        Description = definition.Description;
        ControlType = definition.ControlType;
        Choices = definition.Choices ?? [];
        Minimum = definition.Minimum;
        Maximum = definition.Maximum;
        Step = definition.Step;
        Placeholder = definition.Placeholder;
        _defaultValue = definition.DefaultValue;

        switch (ControlType)
        {
            case OptionControlType.ComboBox:
                selectedChoiceKey = _defaultValue?.ToString();
                break;
            case OptionControlType.Slider:
            case OptionControlType.NumberBox:
                numericValue = TryGetDouble(_defaultValue);
                break;
            case OptionControlType.Toggle:
                boolValue = _defaultValue is bool boolean && boolean;
                break;
            default:
                textValue = _defaultValue?.ToString() ?? string.Empty;
                break;
        }
    }

    public string Key { get; }

    public string DisplayName { get; }

    public string Description { get; }

    public OptionControlType ControlType { get; }

    public IReadOnlyList<OptionChoice> Choices { get; }

    public double Minimum { get; }

    public double Maximum { get; }

    public double Step { get; }

    public string Placeholder { get; }

    public bool IsChoice => ControlType == OptionControlType.ComboBox;

    public bool IsSlider => ControlType == OptionControlType.Slider;

    public bool IsNumber => ControlType == OptionControlType.NumberBox;

    public bool IsToggle => ControlType == OptionControlType.Toggle;

    public bool IsText => ControlType == OptionControlType.Text;

    public bool IsPassword => ControlType == OptionControlType.Password;

    public bool IsColor => ControlType == OptionControlType.Color;

    public bool IsChanged => !Equals(GetValue(), _defaultValue);

    [ObservableProperty]
    private bool isVisible = true;

    [ObservableProperty]
    private string? selectedChoiceKey;

    [ObservableProperty]
    private double numericValue;

    [ObservableProperty]
    private bool boolValue;

    [ObservableProperty]
    private string textValue = string.Empty;

    public object? GetValue() => ControlType switch
    {
        OptionControlType.ComboBox => SelectedChoiceKey,
        OptionControlType.Slider or OptionControlType.NumberBox => NumericValue,
        OptionControlType.Toggle => BoolValue,
        _ => TextValue,
    };

    public void Reset()
    {
        switch (ControlType)
        {
            case OptionControlType.ComboBox:
                SelectedChoiceKey = _defaultValue?.ToString();
                break;
            case OptionControlType.Slider:
            case OptionControlType.NumberBox:
                NumericValue = TryGetDouble(_defaultValue);
                break;
            case OptionControlType.Toggle:
                BoolValue = _defaultValue is bool boolean && boolean;
                break;
            default:
                TextValue = _defaultValue?.ToString() ?? string.Empty;
                break;
        }

        OnPropertyChanged(nameof(IsChanged));
    }

    partial void OnSelectedChoiceKeyChanged(string? value) => OnPropertyChanged(nameof(IsChanged));

    partial void OnNumericValueChanged(double value) => OnPropertyChanged(nameof(IsChanged));

    partial void OnBoolValueChanged(bool value) => OnPropertyChanged(nameof(IsChanged));

    partial void OnTextValueChanged(string value) => OnPropertyChanged(nameof(IsChanged));

    private static double TryGetDouble(object? value) =>
        value switch
        {
            double number => number,
            float number => number,
            int number => number,
            long number => number,
            decimal number => (double)number,
            _ when double.TryParse(value?.ToString(), out var parsed) => parsed,
            _ => 0,
        };
}
