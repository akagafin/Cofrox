using Cofrox.Domain.Enums;

namespace Cofrox.Domain.ValueObjects;

public sealed class FormatOptionDefinition
{
    public FormatOptionDefinition(
        string key,
        string displayName,
        string description,
        OptionControlType controlType,
        object? defaultValue = null,
        IReadOnlyList<OptionChoice>? choices = null,
        double minimum = 0,
        double maximum = 100,
        double step = 1,
        string placeholder = "")
    {
        Key = key;
        DisplayName = displayName;
        Description = description;
        ControlType = controlType;
        DefaultValue = defaultValue;
        Choices = choices;
        Minimum = minimum;
        Maximum = maximum;
        Step = step;
        Placeholder = placeholder;
    }

    public string Key { get; }

    public string DisplayName { get; }

    public string Description { get; }

    public OptionControlType ControlType { get; }

    public object? DefaultValue { get; }

    public IReadOnlyList<OptionChoice>? Choices { get; }

    public double Minimum { get; }

    public double Maximum { get; }

    public double Step { get; }

    public string Placeholder { get; }
}
