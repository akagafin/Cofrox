using Cofrox.Domain.Enums;

namespace Cofrox.Domain.Entities;

public sealed class ConversionJob
{
    public required string Id { get; init; }

    public required FileItem SourceFile { get; init; }

    public required string TargetExtension { get; init; }

    public required string OutputPath { get; init; }

    public required IReadOnlyDictionary<string, object?> Options { get; init; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.Now;

    public ConversionStatus Status { get; set; } = ConversionStatus.Queued;
}
