using System.Text.Json;
using Cofrox.Application.Interfaces;
using Cofrox.Application.Models;
using Cofrox.Domain.Entities;
using Cofrox.Domain.Enums;
using Cofrox.Domain.Interfaces;

namespace Cofrox.Application.Services;

public sealed class QueueManager(ISettingsRepository settingsRepository) : IQueueManager
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task<IReadOnlyList<QueueItemState>> LoadSnapshotAsync(CancellationToken cancellationToken)
    {
        var settings = await settingsRepository.LoadAsync(cancellationToken).ConfigureAwait(false);
        return Deserialize(settings.PersistentQueueStateJson);
    }

    public Task EnqueueAsync(ConversionJob job, CancellationToken cancellationToken) =>
        MutateAsync(
            snapshot =>
            {
                snapshot.RemoveAll(existing => existing.JobId == job.Id);
                snapshot.Add(
                    new QueueItemState(
                        job.Id,
                        job.SourceFile.FileName,
                        job.SourceFile.SourcePath,
                        job.SourceFile.SourceExtension,
                        job.TargetExtension,
                        job.OutputPath,
                        ConversionStatus.Queued,
                        0,
                        false,
                        DateTimeOffset.UtcNow,
                        null,
                        job.Options.ToDictionary(
                            static pair => pair.Key,
                            static pair => pair.Value?.ToString() ?? string.Empty)));
            },
            cancellationToken);

    public Task UpdateStatusAsync(string jobId, ConversionStatus status, string? lastError, CancellationToken cancellationToken) =>
        MutateAsync(
            snapshot => Replace(snapshot, jobId, item => item with { Status = status, LastError = lastError, IsPaused = false }),
            cancellationToken);

    public Task PauseAsync(string jobId, CancellationToken cancellationToken) =>
        MutateAsync(
            snapshot => Replace(snapshot, jobId, item => item with { IsPaused = true, Status = ConversionStatus.Queued }),
            cancellationToken);

    public Task ResumeAsync(string jobId, CancellationToken cancellationToken) =>
        MutateAsync(
            snapshot => Replace(snapshot, jobId, item => item with { IsPaused = false }),
            cancellationToken);

    public Task RetryFailedAsync(string jobId, CancellationToken cancellationToken) =>
        MutateAsync(
            snapshot => Replace(
                snapshot,
                jobId,
                item => item with
                {
                    Status = ConversionStatus.Queued,
                    RetryCount = item.RetryCount + 1,
                    LastError = null,
                    IsPaused = false,
                }),
            cancellationToken);

    public Task RemoveAsync(string jobId, CancellationToken cancellationToken) =>
        MutateAsync(snapshot => snapshot.RemoveAll(item => item.JobId == jobId), cancellationToken);

    private async Task MutateAsync(Action<List<QueueItemState>> mutator, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var settings = await settingsRepository.LoadAsync(cancellationToken).ConfigureAwait(false);
            var snapshot = Deserialize(settings.PersistentQueueStateJson).ToList();
            mutator(snapshot);

            await settingsRepository.SaveAsync(
                settings with { PersistentQueueStateJson = JsonSerializer.Serialize(snapshot, JsonOptions) },
                cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    private static IReadOnlyList<QueueItemState> Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<QueueItemState[]>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static void Replace(List<QueueItemState> snapshot, string jobId, Func<QueueItemState, QueueItemState> projector)
    {
        var item = snapshot.FirstOrDefault(existing => existing.JobId == jobId);
        if (item is null)
        {
            return;
        }

        snapshot.Remove(item);
        snapshot.Add(projector(item));
    }
}
