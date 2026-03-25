using Cofrox.Converters.Infrastructure;
using Cofrox.Domain.Entities;
using Cofrox.Domain.Enums;
using Cofrox.Domain.Interfaces;

namespace Cofrox.Converters.Engines;

public sealed class ImageConversionEngine(
    IFormatCatalog formatCatalog,
    IExternalToolLocator toolLocator,
    IExternalProcessRunner processRunner) : ConversionEngineBase(formatCatalog)
{
    public override bool CanHandle(string sourceExtension, string targetExtension)
    {
        var source = FormatCatalog.GetByExtension(sourceExtension);
        var target = FormatCatalog.GetByExtension(targetExtension);
        return source.Family is FileFamily.Image or FileFamily.RawImage &&
               target.Family is FileFamily.Image or FileFamily.RawImage or FileFamily.Pdf;
    }

    public override async Task<ConversionResult> ConvertAsync(ConversionJob job, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        var magickPath = toolLocator.Resolve("magick");
        if (string.IsNullOrWhiteSpace(magickPath))
        {
            return new ConversionResult
            {
                Status = ConversionStatus.Failed,
                Message = "Bundled ImageMagick binary was not found.",
                Duration = TimeSpan.Zero,
            };
        }

        progress?.Report(0.1);
        var quality = ConversionOptionReader.GetDouble(job.Options, "quality", 85);
        var args = $"\"{job.SourceFile.SourcePath}\" -quality {quality:0} \"{job.OutputPath}\"";
        var startedAt = DateTimeOffset.Now;
        var result = await processRunner.RunAsync(
            new()
            {
                FileName = magickPath,
                Arguments = args,
                WorkingDirectory = Path.GetDirectoryName(job.SourceFile.SourcePath) ?? AppContext.BaseDirectory,
            },
            static _ => null,
            progress,
            cancellationToken).ConfigureAwait(false);

        progress?.Report(result.ExitCode == 0 ? 1 : 0);
        return new ConversionResult
        {
            Status = result.ExitCode == 0 ? ConversionStatus.Completed : ConversionStatus.Failed,
            OutputPath = result.ExitCode == 0 ? job.OutputPath : null,
            Message = result.ExitCode == 0 ? "Image conversion completed." : result.StandardError.Trim(),
            Duration = DateTimeOffset.Now - startedAt,
        };
    }
}
