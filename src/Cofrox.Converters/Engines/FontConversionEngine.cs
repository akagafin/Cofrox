using Cofrox.Domain.Entities;
using Cofrox.Domain.Enums;
using Cofrox.Domain.Interfaces;

namespace Cofrox.Converters.Engines;

public sealed class FontConversionEngine(
    IFormatCatalog formatCatalog,
    IExternalToolLocator toolLocator,
    IExternalProcessRunner processRunner) : ConversionEngineBase(formatCatalog)
{
    public override bool CanHandle(string sourceExtension, string targetExtension) =>
        FormatCatalog.GetByExtension(sourceExtension).Family == FileFamily.Font &&
        FormatCatalog.GetByExtension(targetExtension).Family == FileFamily.Font;

    public override async Task<ConversionResult> ConvertAsync(ConversionJob job, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        var fontToolsPath = toolLocator.Resolve("fonttools");
        if (string.IsNullOrWhiteSpace(fontToolsPath))
        {
            return new ConversionResult
            {
                Status = ConversionStatus.Warning,
                Message = "Font conversion is scaffolded, but no bundled fonttools binary was found.",
                Duration = TimeSpan.Zero,
            };
        }

        var result = await processRunner.RunAsync(
            new()
            {
                FileName = fontToolsPath,
                Arguments = $"\"{job.SourceFile.SourcePath}\" \"{job.OutputPath}\"",
                WorkingDirectory = Path.GetDirectoryName(job.OutputPath) ?? AppContext.BaseDirectory,
            },
            static _ => null,
            progress,
            cancellationToken).ConfigureAwait(false);

        return new ConversionResult
        {
            Status = result.ExitCode == 0 ? ConversionStatus.Completed : ConversionStatus.Warning,
            OutputPath = result.ExitCode == 0 ? job.OutputPath : null,
            Message = result.ExitCode == 0 ? "Font conversion completed." : result.StandardError.Trim(),
            Duration = TimeSpan.Zero,
        };
    }
}
