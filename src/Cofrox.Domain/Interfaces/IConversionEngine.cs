using Cofrox.Domain.Entities;

namespace Cofrox.Domain.Interfaces;

public interface IConversionEngine
{
    bool CanHandle(string sourceExtension, string targetExtension);

    Task<ConversionResult> ConvertAsync(
        ConversionJob job,
        IProgress<double>? progress,
        CancellationToken cancellationToken);
}
