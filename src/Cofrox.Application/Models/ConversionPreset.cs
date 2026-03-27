namespace Cofrox.Application.Models;

public sealed record ConversionPreset(
    string Id,
    string Name,
    string Description,
    string TargetExtension,
    ConversionGoal Goal,
    IReadOnlyDictionary<string, string> Options,
    bool IsBuiltIn);
