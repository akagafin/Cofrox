using Cofrox.Domain.Enums;

namespace Cofrox.Domain.Entities;

public sealed class FileItem
{
    public required string Id { get; init; }

    public required string FileName { get; init; }

    public required string SourcePath { get; init; }

    public required string SourceExtension { get; init; }

    public required FileFamily SourceFamily { get; init; }

    public long FileSizeBytes { get; init; }
}
