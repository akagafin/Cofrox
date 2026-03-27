using System.Globalization;
using System.Text.RegularExpressions;
using Cofrox.Application.Interfaces;
using Cofrox.Application.Models;
using Cofrox.Converters.Infrastructure;
using Cofrox.Domain.Entities;
using Cofrox.Domain.Enums;
using Cofrox.Domain.Interfaces;
using Cofrox.Domain.ValueObjects;

namespace Cofrox.Converters.Engines;

public sealed partial class MultimediaConversionEngine(
    IFormatCatalog formatCatalog,
    IExternalToolLocator toolLocator,
    IExternalProcessRunner processRunner,
    ISystemProfileService systemProfileService,
    ISmartConversionAdvisor smartConversionAdvisor,
    IFFmpegCommandBuilder commandBuilder) : ConversionEngineBase(formatCatalog)
{
    public override bool CanHandle(string sourceExtension, string targetExtension)
    {
        var source = FormatCatalog.GetByExtension(sourceExtension);
        var target = FormatCatalog.GetByExtension(targetExtension);
        return source.Family is FileFamily.Video or FileFamily.Audio &&
               target.Family is FileFamily.Video or FileFamily.Audio;
    }

    public override async Task<ConversionResult> ConvertAsync(ConversionJob job, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        var ffmpegPath = toolLocator.Resolve("ffmpeg");
        if (string.IsNullOrWhiteSpace(ffmpegPath))
        {
            return new ConversionResult
            {
                Status = ConversionStatus.Failed,
                Message = "FFmpeg was not found. Install it separately or place ffmpeg.exe under Tools\\ffmpeg.",
                Duration = TimeSpan.Zero,
            };
        }

        var startedAt = DateTimeOffset.Now;
        var profile = systemProfileService.GetCurrent();
        var effectiveOptions = BuildEffectiveOptions(job);
        var plan = commandBuilder.BuildPlan(job, effectiveOptions, profile);
        var request = new ProcessExecutionRequest
        {
            FileName = ffmpegPath,
            Arguments = plan.Arguments,
            WorkingDirectory = Path.GetDirectoryName(job.SourceFile.SourcePath) ?? AppContext.BaseDirectory,
        };

        var totalDurationMicroseconds = 0d;
        var result = await processRunner.RunAsync(request, ParseProgressLine, progress, cancellationToken).ConfigureAwait(false);
        if (result.TimedOut)
        {
            return new ConversionResult
            {
                Status = ConversionStatus.Failed,
                Message = "FFmpeg conversion timed out after 30 minutes.",
                Duration = DateTimeOffset.Now - startedAt,
            };
        }

        var completed = result.ExitCode == 0;
        var warningMessage = plan.Warnings.Count > 0 ? string.Join(Environment.NewLine, plan.Warnings) : null;
        return new ConversionResult
        {
            Status = completed
                ? (plan.Warnings.Count > 0 ? ConversionStatus.Warning : ConversionStatus.Completed)
                : ConversionStatus.Failed,
            OutputPath = completed ? job.OutputPath : null,
            Message = completed
                ? warningMessage ?? "Media conversion completed."
                : GetFailureMessage(result),
            Duration = DateTimeOffset.Now - startedAt,
        };

        double? ParseProgressLine(string line)
        {
            var durationMatch = DurationPattern().Match(line);
            if (durationMatch.Success &&
                TimeSpan.TryParse(durationMatch.Groups["value"].Value, CultureInfo.InvariantCulture, out var duration) &&
                duration.TotalMilliseconds > 0)
            {
                totalDurationMicroseconds = duration.TotalMilliseconds * 1000d;
                return null;
            }

            var outTimeMatch = ProgressPattern().Match(line);
            if (!outTimeMatch.Success ||
                !double.TryParse(outTimeMatch.Groups["value"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var processedMicroseconds) ||
                totalDurationMicroseconds <= 0)
            {
                return null;
            }

            return Math.Clamp(processedMicroseconds / totalDurationMicroseconds, 0, 0.99);
        }
    }

    private IReadOnlyDictionary<string, object?> BuildEffectiveOptions(ConversionJob job)
    {
        var merged = MergeVideoPresetOptions(job.Options);
        var goal = ResolveGoal(merged);
        var recommendation = smartConversionAdvisor.Recommend(job.SourceFile, job.TargetExtension, goal, merged);
        return recommendation.RecommendedOptions;
    }

    private static Dictionary<string, object?> MergeVideoPresetOptions(IReadOnlyDictionary<string, object?> options)
    {
        var merged = new Dictionary<string, object?>(options, StringComparer.OrdinalIgnoreCase);
        var preset = ConversionOptionReader.GetString(merged, "video_preset", "custom");
        if (string.Equals(preset, "custom", StringComparison.OrdinalIgnoreCase))
        {
            return merged;
        }

        switch (preset)
        {
            case "hb_web_720p":
                merged["resolution"] = "720p";
                merged["video_codec"] = "h264";
                merged["quality_mode"] = "crf";
                merged["quality_value"] = 23d;
                merged["frame_rate"] = "30";
                merged["conversion_goal"] = ConversionGoal.Size.ToString();
                break;
            case "hb_fast_1080p30":
                merged["resolution"] = "1080p";
                merged["video_codec"] = "h264";
                merged["quality_mode"] = "crf";
                merged["quality_value"] = 22d;
                merged["frame_rate"] = "30";
                merged["conversion_goal"] = ConversionGoal.Balanced.ToString();
                break;
            case "hb_hq_1080p30":
                merged["resolution"] = "1080p";
                merged["video_codec"] = "h264";
                merged["quality_mode"] = "crf";
                merged["quality_value"] = 18d;
                merged["frame_rate"] = "30";
                merged["conversion_goal"] = ConversionGoal.Quality.ToString();
                break;
            case "hb_super_hq_1080p":
                merged["resolution"] = "1080p";
                merged["video_codec"] = "h264";
                merged["quality_mode"] = "crf";
                merged["quality_value"] = 16d;
                merged["frame_rate"] = "original";
                merged["conversion_goal"] = ConversionGoal.Quality.ToString();
                break;
            case "hb_anime_1080p":
                merged["resolution"] = "1080p";
                merged["video_codec"] = "h264";
                merged["quality_mode"] = "crf";
                merged["quality_value"] = 20d;
                merged["frame_rate"] = "original";
                merged["conversion_goal"] = ConversionGoal.Quality.ToString();
                break;
            case "hb_fast_4k":
                merged["resolution"] = "2160p";
                merged["video_codec"] = "h265";
                merged["quality_mode"] = "crf";
                merged["quality_value"] = 24d;
                merged["frame_rate"] = "30";
                merged["conversion_goal"] = ConversionGoal.Size.ToString();
                break;
            case "hb_hq_4k":
                merged["resolution"] = "2160p";
                merged["video_codec"] = "h265";
                merged["quality_mode"] = "crf";
                merged["quality_value"] = 20d;
                merged["frame_rate"] = "original";
                merged["conversion_goal"] = ConversionGoal.Quality.ToString();
                break;
        }

        return merged;
    }

    private static ConversionGoal ResolveGoal(IReadOnlyDictionary<string, object?> options)
    {
        if (options.TryGetValue("conversion_goal", out var explicitGoal) &&
            explicitGoal is not null &&
            Enum.TryParse<ConversionGoal>(explicitGoal.ToString(), true, out var parsedGoal))
        {
            return parsedGoal;
        }

        return ConversionOptionReader.GetString(options, "video_preset", "custom") switch
        {
            "hb_web_720p" => ConversionGoal.Size,
            "hb_fast_4k" => ConversionGoal.Size,
            "hb_hq_1080p30" or "hb_super_hq_1080p" or "hb_anime_1080p" or "hb_hq_4k" => ConversionGoal.Quality,
            _ => ConversionGoal.Balanced,
        };
    }

    private static string GetFailureMessage(ProcessExecutionResult result)
    {
        if (!string.IsNullOrWhiteSpace(result.StandardError))
        {
            return result.StandardError.Trim();
        }

        if (!string.IsNullOrWhiteSpace(result.StandardOutput))
        {
            return result.StandardOutput.Trim();
        }

        return "FFmpeg exited without a detailed error message.";
    }

    [GeneratedRegex(@"out_time_ms=(?<value>\d+)")]
    private static partial Regex ProgressPattern();

    [GeneratedRegex(@"Duration:\s(?<value>\d{2}:\d{2}:\d{2}\.\d+)")]
    private static partial Regex DurationPattern();
}
