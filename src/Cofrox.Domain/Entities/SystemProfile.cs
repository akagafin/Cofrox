namespace Cofrox.Domain.Entities;

public sealed class SystemProfile
{
    public ulong TotalPhysicalMemoryBytes { get; init; }

    public bool IsLowMemoryDevice => TotalPhysicalMemoryBytes > 0 && TotalPhysicalMemoryBytes < 4UL * 1024UL * 1024UL * 1024UL;

    public int RecommendedMediaThreads => IsLowMemoryDevice ? 2 : Math.Min(Environment.ProcessorCount, 8);

    public int RecommendedParallelConversions => IsLowMemoryDevice ? 1 : 2;
}
