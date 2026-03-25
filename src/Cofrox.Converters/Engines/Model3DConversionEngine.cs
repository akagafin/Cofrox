using Cofrox.Domain.Entities;
using Cofrox.Domain.Enums;
using Cofrox.Domain.Interfaces;

namespace Cofrox.Converters.Engines;

public sealed class Model3DConversionEngine(IFormatCatalog formatCatalog) : ConversionEngineBase(formatCatalog)
{
    public override bool CanHandle(string sourceExtension, string targetExtension) =>
        FormatCatalog.GetByExtension(sourceExtension).Family == FileFamily.Model3D &&
        FormatCatalog.GetByExtension(targetExtension).Family == FileFamily.Model3D;

    public override Task<ConversionResult> ConvertAsync(ConversionJob job, IProgress<double>? progress, CancellationToken cancellationToken) =>
        Task.FromResult(new ConversionResult
        {
            Status = ConversionStatus.Warning,
            Message = "3D conversion is scaffolded for AssimpNet, but the managed bridge is not wired yet.",
            Duration = TimeSpan.Zero,
        });
}
