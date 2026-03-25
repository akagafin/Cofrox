using Cofrox.Domain.Entities;

namespace Cofrox.Domain.Interfaces;

public interface IConversionCoordinator
{
    Task<ConversionResult> ConvertAsync(
        ConversionJob job,
        IProgress<double>? progress,
        CancellationToken cancellationToken);
}
