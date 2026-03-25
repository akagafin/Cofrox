using System.Text;
using System.Text.RegularExpressions;
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
    ISystemProfileService systemProfileService) : ConversionEngineBase(formatCatalog)
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
                Message = "Bundled FFmpeg binary was not found.",
                Duration = TimeSpan.Zero,
            };
        }

        var startedAt = DateTimeOffset.Now;
        var args = BuildArguments(job, systemProfileService.GetCurrent());
        var request = new ProcessExecutionRequest
        {
            FileName = ffmpegPath,
            Arguments = args,
            WorkingDirectory = Path.GetDirectoryName(job.SourceFile.SourcePath) ?? AppContext.BaseDirectory,
        };

        var result = await processRunner.RunAsync(request, ParseProgress, progress, cancellationToken).ConfigureAwait(false);
        if (result.TimedOut)
        {
            return new ConversionResult
            {
                Status = ConversionStatus.Failed,
                Message = "FFmpeg conversion timed out after 30 minutes.",
                Duration = DateTimeOffset.Now - startedAt,
            };
        }

        return new ConversionResult
        {
            Status = result.ExitCode == 0 ? ConversionStatus.Completed : ConversionStatus.Failed,
            OutputPath = result.ExitCode == 0 ? job.OutputPath : null,
            Message = result.ExitCode == 0 ? "Media conversion completed." : result.StandardError.Trim(),
            Duration = DateTimeOffset.Now - startedAt,
        };
    }

    private static string BuildArguments(ConversionJob job, SystemProfile profile)
    {
        var targetFormat = Path.GetExtension(job.OutputPath).TrimStart('.').ToLowerInvariant();
        var builder = new StringBuilder();
        builder.Append("-y ");
        builder.Append("-i ").Append('"').Append(job.SourceFile.SourcePath).Append("\" ");

        if (profile.IsLowMemoryDevice)
        {
            builder.Append("-preset ultrafast -threads 2 ");
        }

        var trimStart = ConversionOptionReader.GetString(job.Options, "trim_start");
        var trimEnd = ConversionOptionReader.GetString(job.Options, "trim_end");
        if (!string.IsNullOrWhiteSpace(trimStart))
        {
            builder.Append("-ss ").Append(trimStart).Append(' ');
        }

        if (!string.IsNullOrWhiteSpace(trimEnd))
        {
            builder.Append("-to ").Append(trimEnd).Append(' ');
        }

        var resolution = ConversionOptionReader.GetString(job.Options, "resolution", "original");
        var filters = GetVideoFilters(job.Options, resolution);
        if (!string.IsNullOrWhiteSpace(filters))
        {
            builder.Append("-vf ").Append('"').Append(filters).Append("\" ");
        }

        var audioMode = ConversionOptionReader.GetString(job.Options, "audio_mode", "keep");
        if (string.Equals(audioMode, "remove", StringComparison.OrdinalIgnoreCase))
        {
            builder.Append("-an ");
        }
        else if (string.Equals(audioMode, "reencode", StringComparison.OrdinalIgnoreCase))
        {
            builder.Append("-c:a ").Append(ConvertAudioCodec(ConversionOptionReader.GetString(job.Options, "audio_codec", "aac"))).Append(' ');
            builder.Append("-b:a ").Append(ConversionOptionReader.GetString(job.Options, "audio_bitrate", "128")).Append("k ");
        }

        if (targetFormat is "mp4" or "mkv" or "avi" or "mov" or "webm" or "gif")
        {
            builder.Append("-c:v ").Append(ConvertVideoCodec(ConversionOptionReader.GetString(job.Options, "video_codec", "h264"))).Append(' ');

            var qualityMode = ConversionOptionReader.GetString(job.Options, "quality_mode", "crf");
            if (string.Equals(qualityMode, "bitrate", StringComparison.OrdinalIgnoreCase))
            {
                builder.Append("-b:v ").Append((int)ConversionOptionReader.GetDouble(job.Options, "target_bitrate", 2500)).Append("k ");
            }
            else
            {
                builder.Append("-crf ").Append((int)ConversionOptionReader.GetDouble(job.Options, "quality_value", 23)).Append(' ');
            }
        }

        builder.Append("-progress pipe:2 ");
        builder.Append('"').Append(job.OutputPath).Append('"');
        return builder.ToString();
    }

    private static string GetVideoFilters(IReadOnlyDictionary<string, object?> options, string resolution)
    {
        var filters = new List<string>();
        var rotate = ConversionOptionReader.GetString(options, "rotate", "none");
        filters.Add(rotate switch
        {
            "cw90" => "transpose=1",
            "ccw90" => "transpose=2",
            "180" => "transpose=2,transpose=2",
            "flip_h" => "hflip",
            "flip_v" => "vflip",
            _ => string.Empty,
        });

        filters.Add(resolution switch
        {
            "4320p" => "scale=7680:4320",
            "2160p" => "scale=3840:2160",
            "1440p" => "scale=2560:1440",
            "1080p" => "scale=1920:1080",
            "720p" => "scale=1280:720",
            "480p" => "scale=854:480",
            "360p" => "scale=640:360",
            _ => string.Empty,
        });

        var speed = ConversionOptionReader.GetDouble(options, "speed", 1);
        if (Math.Abs(speed - 1) > 0.001)
        {
            filters.Add($"setpts={1 / speed:0.###}*PTS");
        }

        return string.Join(",", filters.Where(static value => !string.IsNullOrWhiteSpace(value)));
    }

    private static string ConvertVideoCodec(string codec) => codec switch
    {
        "h265" => "libx265",
        "av1" => "libaom-av1",
        "vp8" => "libvpx",
        "vp9" => "libvpx-vp9",
        "mpeg4" => "mpeg4",
        "divx" => "mpeg4",
        _ => "libx264",
    };

    private static string ConvertAudioCodec(string codec) => codec switch
    {
        "mp3" => "libmp3lame",
        "ogg" => "libvorbis",
        "opus" => "libopus",
        "alac" => "alac",
        _ => "aac",
    };

    private static double? ParseProgress(string line)
    {
        var match = ProgressPattern().Match(line);
        if (!match.Success || !double.TryParse(match.Groups["value"].Value, out var milliseconds))
        {
            return null;
        }

        return Math.Clamp(milliseconds / 600_000_000d, 0, 0.99);
    }

    [GeneratedRegex(@"out_time_ms=(?<value>\d+)")]
    private static partial Regex ProgressPattern();
}
