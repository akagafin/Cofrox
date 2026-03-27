using Cofrox.Domain.Entities;
using Cofrox.Domain.Enums;
using Cofrox.Domain.Interfaces;

namespace Cofrox.Converters.Engines;

public sealed class DocumentConversionEngine(
    IFormatCatalog formatCatalog,
    IExternalToolLocator toolLocator,
    IExternalProcessRunner processRunner) : ConversionEngineBase(formatCatalog)
{
    public override bool CanHandle(string sourceExtension, string targetExtension)
    {
        var source = FormatCatalog.GetByExtension(sourceExtension);
        var target = FormatCatalog.GetByExtension(targetExtension);
        return source.Family is FileFamily.Document or FileFamily.Pdf or FileFamily.Spreadsheet or FileFamily.Ebook &&
               target.Family is FileFamily.Document or FileFamily.Pdf or FileFamily.Ebook or FileFamily.Spreadsheet or FileFamily.Data or FileFamily.Image;
    }

    public override async Task<ConversionResult> ConvertAsync(ConversionJob job, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        progress?.Report(0.1);
        var targetExtension = Path.GetExtension(job.OutputPath).TrimStart('.').ToLowerInvariant();
        var startedAt = DateTimeOffset.Now;

        if (targetExtension == "pdf")
        {
            var sofficePath = toolLocator.Resolve("libreoffice");
            if (!string.IsNullOrWhiteSpace(sofficePath))
            {
                var outputDirectory = Path.GetDirectoryName(job.OutputPath) ?? AppContext.BaseDirectory;
                var args = $"--headless --convert-to pdf --outdir \"{outputDirectory}\" \"{job.SourceFile.SourcePath}\"";
                var result = await processRunner.RunAsync(
                    new()
                    {
                        FileName = sofficePath,
                        Arguments = args,
                        WorkingDirectory = outputDirectory,
                    },
                    static _ => null,
                    progress,
                    cancellationToken).ConfigureAwait(false);

                return new ConversionResult
                {
                    Status = result.ExitCode == 0 ? ConversionStatus.Completed : ConversionStatus.Warning,
                    OutputPath = result.ExitCode == 0 ? job.OutputPath : null,
                    Message = result.ExitCode == 0
                        ? "LibreOffice conversion completed."
                        : "LibreOffice failed. Managed document fallback still needs the Syncfusion bridge.",
                    Duration = DateTimeOffset.Now - startedAt,
                };
            }
        }

        var pandocPath = toolLocator.Resolve("pandoc");
        if (!string.IsNullOrWhiteSpace(pandocPath))
        {
            var args = $"\"{job.SourceFile.SourcePath}\" -o \"{job.OutputPath}\"";
            var result = await processRunner.RunAsync(
                new()
                {
                    FileName = pandocPath,
                    Arguments = args,
                    WorkingDirectory = Path.GetDirectoryName(job.OutputPath) ?? AppContext.BaseDirectory,
                },
                static _ => null,
                progress,
                cancellationToken).ConfigureAwait(false);

            return new ConversionResult
            {
                Status = result.ExitCode == 0 ? ConversionStatus.Completed : ConversionStatus.Warning,
                OutputPath = result.ExitCode == 0 ? job.OutputPath : null,
                Message = result.ExitCode == 0
                    ? "Pandoc conversion completed."
                    : "Pandoc failed and the native document bridge is still pending.",
                Duration = DateTimeOffset.Now - startedAt,
            };
        }

        return new ConversionResult
        {
            Status = ConversionStatus.Warning,
            Message = "No supported document engine was found. Install LibreOffice or Pandoc, or place them under Tools\\libreoffice / Tools\\pandoc.",
            Duration = DateTimeOffset.Now - startedAt,
        };
    }
}
