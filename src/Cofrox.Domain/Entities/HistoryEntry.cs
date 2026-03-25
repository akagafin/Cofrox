using Cofrox.Domain.Enums;

namespace Cofrox.Domain.Entities;

public sealed class HistoryEntry
{
    public long Id { get; init; }

    public required string FileName { get; init; }

    public required string SourceFormat { get; init; }

    public required string TargetFormat { get; init; }

    public required DateTimeOffset ConvertedAt { get; init; }

    public required HistoryEntryStatus Status { get; init; }

    public string? OutputPath { get; init; }

    public string? Message { get; init; }
}
