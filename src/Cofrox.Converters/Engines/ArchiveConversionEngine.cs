using Cofrox.Domain.Entities;
using Cofrox.Domain.Enums;
using Cofrox.Domain.Interfaces;

namespace Cofrox.Converters.Engines;

public sealed class ArchiveConversionEngine(
    IFormatCatalog formatCatalog,
    IExternalToolLocator toolLocator,
    IExternalProcessRunner processRunner) : ConversionEngineBase(formatCatalog)
{
    public override bool CanHandle(string sourceExtension, string targetExtension) =>
        FormatCatalog.GetByExtension(sourceExtension).Family == FileFamily.Archive &&
        FormatCatalog.GetByExtension(targetExtension).Family == FileFamily.Archive;

    public override async Task<ConversionResult> ConvertAsync(ConversionJob job, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        var sevenZipPath = toolLocator.Resolve("7zip");
        if (string.IsNullOrWhiteSpace(sevenZipPath))
        {
            return new ConversionResult
            {
                Status = ConversionStatus.Failed,
                Message = "7-Zip was not found. Install it separately or place 7z.exe under Tools\\7zip.",
                Duration = TimeSpan.Zero,
            };
        }

        progress?.Report(0.1);
        var startedAt = DateTimeOffset.Now;
        var outputExtension = Path.GetExtension(job.OutputPath).TrimStart('.').ToLowerInvariant();
        var args = $"a -t{outputExtension} \"{job.OutputPath}\" \"{job.SourceFile.SourcePath}\"";
        var result = await processRunner.RunAsync(
            new()
            {
                FileName = sevenZipPath,
                Arguments = args,
                WorkingDirectory = Path.GetDirectoryName(job.OutputPath) ?? AppContext.BaseDirectory,
            },
            static _ => null,
            progress,
            cancellationToken).ConfigureAwait(false);

        return new ConversionResult
        {
            Status = result.ExitCode == 0 ? ConversionStatus.Completed : ConversionStatus.Failed,
            OutputPath = result.ExitCode == 0 ? job.OutputPath : null,
            Message = result.ExitCode == 0 ? "Archive conversion completed." : result.StandardError.Trim(),
            Duration = DateTimeOffset.Now - startedAt,
        };
    }
}
