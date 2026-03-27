using Xunit;

namespace Cofrox.Application.Tests;

public sealed class ConversionResilienceIntegrationTests
{
    [Fact(Skip = "Requires local FFmpeg/ImageMagick/Pandoc binaries and large fixture files.")]
    public void LargeFiles_AboveTenGigabytes_AreProcessedWithinConfiguredQueueLimits()
    {
    }

    [Fact(Skip = "Requires corrupted media/document fixtures and external engines.")]
    public void CorruptedFiles_AreReportedAsFailuresWithoutCrashingTheQueue()
    {
    }

    [Fact(Skip = "Requires unsupported fixture matrix and engine bundles.")]
    public void UnsupportedFormats_ReturnWarningsInsteadOfUnhandledExceptions()
    {
    }

    [Fact(Skip = "Requires cancellable long-running external conversions.")]
    public void InterruptedConversions_AreMarkedCancelled_AndLeaveTempCleanupSafe()
    {
    }

    [Fact(Skip = "Requires multi-core hardware and bundled engines.")]
    public void ConcurrentJobs_RespectParallelLimits_AndDoNotDeadlock()
    {
    }
}
