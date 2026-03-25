using Cofrox.Domain.Enums;

namespace Cofrox.Domain.Entities;

public sealed class ConversionResult
{
    public required ConversionStatus Status { get; init; }

    public string? OutputPath { get; init; }

    public string? Message { get; init; }

    public TimeSpan Duration { get; init; }

    public bool Openable => Status is ConversionStatus.Completed or ConversionStatus.Warning && !string.IsNullOrWhiteSpace(OutputPath);
}
