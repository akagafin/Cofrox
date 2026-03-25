namespace Cofrox.Domain.Enums;

public enum ConversionStatus
{
    Queued,
    Preparing,
    Running,
    Completed,
    Warning,
    Failed,
    Cancelled,
}
