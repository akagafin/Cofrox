namespace Cofrox.Domain.ValueObjects;

public sealed class ProcessExecutionRequest
{
    public required string FileName { get; init; }

    public required string Arguments { get; init; }

    public required string WorkingDirectory { get; init; }

    public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(30);
}
