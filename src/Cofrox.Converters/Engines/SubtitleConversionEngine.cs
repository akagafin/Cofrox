using Cofrox.Domain.Entities;
using Cofrox.Domain.Enums;
using Cofrox.Domain.Interfaces;

namespace Cofrox.Converters.Engines;

public sealed class SubtitleConversionEngine(IFormatCatalog formatCatalog) : ConversionEngineBase(formatCatalog)
{
    public override bool CanHandle(string sourceExtension, string targetExtension) =>
        FormatCatalog.GetByExtension(sourceExtension).Family == FileFamily.Subtitle &&
        FormatCatalog.GetByExtension(targetExtension).Family == FileFamily.Subtitle;

    public override Task<ConversionResult> ConvertAsync(ConversionJob job, IProgress<double>? progress, CancellationToken cancellationToken) =>
        Task.FromResult(new ConversionResult
        {
            Status = ConversionStatus.Warning,
            Message = "Subtitle conversion is scaffolded, but the plain-text parser bridge is still pending.",
            Duration = TimeSpan.Zero,
        });
}
