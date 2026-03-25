using Cofrox.Domain.Entities;
using Cofrox.Domain.Interfaces;

namespace Cofrox.Converters.Engines;

public abstract class ConversionEngineBase(IFormatCatalog formatCatalog) : IConversionEngine
{
    protected IFormatCatalog FormatCatalog { get; } = formatCatalog;

    public abstract bool CanHandle(string sourceExtension, string targetExtension);

    public abstract Task<ConversionResult> ConvertAsync(
        ConversionJob job,
        IProgress<double>? progress,
        CancellationToken cancellationToken);
}
