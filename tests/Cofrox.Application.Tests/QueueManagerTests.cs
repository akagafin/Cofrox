using Cofrox.Application.Models;
using Cofrox.Application.Services;
using Cofrox.Application.Tests.Support;
using Cofrox.Domain.Entities;
using Cofrox.Domain.Enums;
using Xunit;

namespace Cofrox.Application.Tests;

public sealed class QueueManagerTests
{
    [Fact]
    public async Task EnqueueAndRetryFailedAsync_PreservesQueueState()
    {
        var repository = new InMemorySettingsRepository();
        var queueManager = new QueueManager(repository);
        var job = CreateJob();

        await queueManager.EnqueueAsync(job, CancellationToken.None);
        await queueManager.UpdateStatusAsync(job.Id, ConversionStatus.Failed, "Encoder error", CancellationToken.None);
        await queueManager.RetryFailedAsync(job.Id, CancellationToken.None);

        var snapshot = await queueManager.LoadSnapshotAsync(CancellationToken.None);
        var item = Assert.Single(snapshot);

        Assert.Equal(ConversionStatus.Queued, item.Status);
        Assert.Equal(1, item.RetryCount);
        Assert.Null(item.LastError);
    }

    [Fact]
    public async Task PauseAndResume_TogglesPauseFlag()
    {
        var repository = new InMemorySettingsRepository();
        var queueManager = new QueueManager(repository);
        var job = CreateJob();

        await queueManager.EnqueueAsync(job, CancellationToken.None);
        await queueManager.PauseAsync(job.Id, CancellationToken.None);
        await queueManager.ResumeAsync(job.Id, CancellationToken.None);

        var snapshot = await queueManager.LoadSnapshotAsync(CancellationToken.None);
        var item = Assert.Single(snapshot);

        Assert.False(item.IsPaused);
    }

    private static ConversionJob CreateJob() =>
        new()
        {
            Id = Guid.NewGuid().ToString("N"),
            SourceFile = new FileItem
            {
                Id = Guid.NewGuid().ToString("N"),
                FileName = "clip.mov",
                SourcePath = @"C:\fixtures\clip.mov",
                SourceExtension = "mov",
                SourceFamily = FileFamily.Video,
                FileSizeBytes = 42,
            },
            TargetExtension = "mp4",
            OutputPath = @"C:\output\clip.mp4",
            Options = new Dictionary<string, object?> { ["video_codec"] = "h264" },
        };
}
