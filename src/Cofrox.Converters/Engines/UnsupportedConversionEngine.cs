using Cofrox.Domain.Entities;
using Cofrox.Domain.Enums;
using Cofrox.Domain.Interfaces;

namespace Cofrox.Converters.Engines;

public sealed class UnsupportedConversionEngine(IFormatCatalog formatCatalog) : ConversionEngineBase(formatCatalog)
{
    public override bool CanHandle(string sourceExtension, string targetExtension) => true;

    public override Task<ConversionResult> ConvertAsync(ConversionJob job, IProgress<double>? progress, CancellationToken cancellationToken) =>
        Task.FromResult(new ConversionResult
        {
            Status = ConversionStatus.Warning,
            Message = "This conversion path is not supported yet by the installed engines.",
            Duration = TimeSpan.Zero,
        });
}
