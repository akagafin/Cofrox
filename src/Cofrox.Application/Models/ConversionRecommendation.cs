namespace Cofrox.Application.Models;

public sealed record ConversionRecommendation(
    string TargetExtension,
    ConversionGoal Goal,
    string RecommendedPresetId,
    IReadOnlyDictionary<string, object?> RecommendedOptions,
    IReadOnlyList<string> Warnings,
    double EstimatedCompressionRatio);
