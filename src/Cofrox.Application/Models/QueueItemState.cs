using Cofrox.Domain.Enums;

namespace Cofrox.Application.Models;

public sealed record QueueItemState(
    string JobId,
    string FileName,
    string SourcePath,
    string SourceExtension,
    string TargetExtension,
    string OutputPath,
    ConversionStatus Status,
    int RetryCount,
    bool IsPaused,
    DateTimeOffset EnqueuedAt,
    string? LastError,
    IReadOnlyDictionary<string, string> Options);
