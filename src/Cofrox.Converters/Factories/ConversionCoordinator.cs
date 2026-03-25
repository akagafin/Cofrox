using Cofrox.Domain.Entities;
using Cofrox.Domain.Enums;
using Cofrox.Domain.Interfaces;

namespace Cofrox.Converters.Factories;

public sealed class ConversionCoordinator(
    IEnumerable<IConversionEngine> engines,
    IHistoryRepository historyRepository,
    ISettingsRepository settingsRepository,
    ISystemProfileService systemProfileService,
    ITempFileService tempFileService) : IConversionCoordinator
{
    private readonly List<IConversionEngine> _engines = engines.ToList();
    private SemaphoreSlim? _semaphore;

    public async Task<ConversionResult> ConvertAsync(ConversionJob job, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        var settings = await settingsRepository.LoadAsync(cancellationToken).ConfigureAwait(false);
        await historyRepository.InitializeAsync(cancellationToken).ConfigureAwait(false);

        _semaphore ??= new SemaphoreSlim(
            ResolveParallelLimit(settings.MaxParallelConversions, systemProfileService.GetCurrent()),
            2);

        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var engine = _engines.First(candidate => candidate.CanHandle(job.SourceFile.SourceExtension, job.TargetExtension));
            progress?.Report(0.02);
            var result = await engine.ConvertAsync(job, progress, cancellationToken).ConfigureAwait(false);

            if (settings.SaveHistory)
            {
                await historyRepository.AddAsync(
                    new HistoryEntry
                    {
                        FileName = job.SourceFile.FileName,
                        SourceFormat = job.SourceFile.SourceExtension,
                        TargetFormat = job.TargetExtension,
                        ConvertedAt = DateTimeOffset.Now,
                        Status = result.Status is ConversionStatus.Completed or ConversionStatus.Warning
                            ? HistoryEntryStatus.Success
                            : HistoryEntryStatus.Failed,
                        OutputPath = result.OutputPath,
                        Message = result.Message,
                    },
                    cancellationToken).ConfigureAwait(false);
            }

            if (settings.AutoDeleteTempFiles)
            {
                await tempFileService.CleanupJobAsync(job.Id, cancellationToken).ConfigureAwait(false);
            }

            return result;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private static int ResolveParallelLimit(int configuredValue, SystemProfile profile) =>
        configuredValue switch
        {
            <= 0 => profile.RecommendedParallelConversions,
            > 2 => 2,
            _ => configuredValue,
        };
}
