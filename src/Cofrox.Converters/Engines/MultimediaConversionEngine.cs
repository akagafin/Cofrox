using System.Globalization;
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
        var profile = systemProfileService.GetCurrent();
        var effective = MergeVideoPresetOptions(job.Options);
        var args = BuildArguments(job, effective, profile);
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

    private static Dictionary<string, object?> MergeVideoPresetOptions(IReadOnlyDictionary<string, object?> options)
    {
        var merged = new Dictionary<string, object?>(options);
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
                merged["quality_value"] = 23.0;
                merged["frame_rate"] = "30";
                break;
            case "hb_fast_1080p30":
                merged["resolution"] = "1080p";
                merged["video_codec"] = "h264";
                merged["quality_mode"] = "crf";
                merged["quality_value"] = 22.0;
                merged["frame_rate"] = "30";
                break;
            case "hb_hq_1080p30":
                merged["resolution"] = "1080p";
                merged["video_codec"] = "h264";
                merged["quality_mode"] = "crf";
                merged["quality_value"] = 18.0;
                merged["frame_rate"] = "30";
                break;
            case "hb_super_hq_1080p":
                merged["resolution"] = "1080p";
                merged["video_codec"] = "h264";
                merged["quality_mode"] = "crf";
                merged["quality_value"] = 16.0;
                merged["frame_rate"] = "original";
                break;
            case "hb_anime_1080p":
                merged["resolution"] = "1080p";
                merged["video_codec"] = "h264";
                merged["quality_mode"] = "crf";
                merged["quality_value"] = 20.0;
                merged["frame_rate"] = "original";
                break;
            case "hb_fast_4k":
                merged["resolution"] = "2160p";
                merged["video_codec"] = "h265";
                merged["quality_mode"] = "crf";
                merged["quality_value"] = 24.0;
                merged["frame_rate"] = "30";
                break;
            case "hb_hq_4k":
                merged["resolution"] = "2160p";
                merged["video_codec"] = "h265";
                merged["quality_mode"] = "crf";
                merged["quality_value"] = 20.0;
                merged["frame_rate"] = "original";
                break;
        }

        return merged;
    }

    private static string BuildArguments(ConversionJob job, IReadOnlyDictionary<string, object?> o, SystemProfile profile)
    {
        var targetFormat = Path.GetExtension(job.OutputPath).TrimStart('.').ToLowerInvariant();
        if (IsAudioOnlyContainer(targetFormat))
        {
            return BuildAudioOnlyArguments(job, o, profile);
        }

        var builder = new StringBuilder();
        builder.Append("-y ");
        builder.Append("-i ").Append('"').Append(job.SourceFile.SourcePath).Append("\" ");

        if (profile.IsLowMemoryDevice)
        {
            builder.Append("-threads 2 ");
        }

        var trimStart = ConversionOptionReader.GetString(o, "trim_start");
        var trimEnd = ConversionOptionReader.GetString(o, "trim_end");
        if (!string.IsNullOrWhiteSpace(trimStart))
        {
            builder.Append("-ss ").Append(trimStart).Append(' ');
        }

        if (!string.IsNullOrWhiteSpace(trimEnd))
        {
            builder.Append("-to ").Append(trimEnd).Append(' ');
        }

        var resolution = ConversionOptionReader.GetString(o, "resolution", "original");
        var filters = GetVideoFilters(job, o, resolution);
        if (!string.IsNullOrWhiteSpace(filters))
        {
            builder.Append("-vf ").Append('"').Append(filters).Append("\" ");
        }

        var audioMode = ConversionOptionReader.GetString(o, "audio_mode", "keep");
        if (string.Equals(audioMode, "remove", StringComparison.OrdinalIgnoreCase))
        {
            builder.Append("-an ");
        }
        else if (string.Equals(audioMode, "reencode", StringComparison.OrdinalIgnoreCase))
        {
            builder.Append("-c:a ").Append(ConvertAudioCodec(ConversionOptionReader.GetString(o, "audio_codec", "aac"))).Append(' ');
            builder.Append("-b:a ").Append(ConversionOptionReader.GetString(o, "audio_bitrate", "128")).Append("k ");
        }
        else
        {
            builder.Append("-c:a copy ");
        }

        if (targetFormat is "mp4" or "mkv" or "avi" or "mov" or "webm" or "gif")
        {
            var codecKey = ConversionOptionReader.GetString(o, "video_codec", "h264");
            var encoderMode = ConversionOptionReader.GetString(o, "video_encoder", "software");
            var speed = ConversionOptionReader.GetString(o, "encoding_speed", "medium");

            if (profile.IsLowMemoryDevice && string.Equals(encoderMode, "software", StringComparison.OrdinalIgnoreCase))
            {
                speed = "ultrafast";
            }

            var (videoEncoder, usesSoftwareCodec) = ResolveVideoEncoder(codecKey, encoderMode);
            builder.Append("-c:v ").Append(videoEncoder).Append(' ');

            if (usesSoftwareCodec)
            {
                builder.Append("-preset ").Append(speed).Append(' ');
            }
            else if (string.Equals(encoderMode, "nvenc", StringComparison.OrdinalIgnoreCase))
            {
                builder.Append("-preset p4 ");
            }

            var qualityMode = ConversionOptionReader.GetString(o, "quality_mode", "crf");
            var q = (int)ConversionOptionReader.GetDouble(o, "quality_value", 23);
            var bitrateK = (int)ConversionOptionReader.GetDouble(o, "target_bitrate", 2500);

            if (string.Equals(qualityMode, "bitrate", StringComparison.OrdinalIgnoreCase))
            {
                builder.Append("-b:v ").Append(bitrateK).Append("k ");
            }
            else
            {
                AppendConstantQualityArgs(builder, encoderMode, videoEncoder, q);
            }

            if (ShouldUseYuv420p(targetFormat, videoEncoder))
            {
                builder.Append("-pix_fmt yuv420p ");
            }

            var fps = ConversionOptionReader.GetString(o, "frame_rate", "original");
            if (!string.Equals(fps, "original", StringComparison.OrdinalIgnoreCase) &&
                double.TryParse(fps, NumberStyles.Float, CultureInfo.InvariantCulture, out var fpsVal))
            {
                builder.Append(CultureInfo.InvariantCulture, $"-r {fpsVal} ");
            }
        }

        builder.Append("-progress pipe:2 ");
        builder.Append('"').Append(job.OutputPath).Append('"');
        return builder.ToString();
    }

    private static bool IsAudioOnlyContainer(string targetFormat) =>
        targetFormat is "mp3" or "aac" or "m4a" or "opus" or "ogg" or "flac" or "wav";

    private static string BuildAudioOnlyArguments(ConversionJob job, IReadOnlyDictionary<string, object?> o, SystemProfile profile)
    {
        var targetFormat = Path.GetExtension(job.OutputPath).TrimStart('.').ToLowerInvariant();
        var builder = new StringBuilder();
        builder.Append("-y ");
        builder.Append("-i ").Append('"').Append(job.SourceFile.SourcePath).Append("\" ");

        if (profile.IsLowMemoryDevice)
        {
            builder.Append("-threads 2 ");
        }

        var trimStart = ConversionOptionReader.GetString(o, "trim_start");
        var trimEnd = ConversionOptionReader.GetString(o, "trim_end");
        if (!string.IsNullOrWhiteSpace(trimStart))
        {
            builder.Append("-ss ").Append(trimStart).Append(' ');
        }

        if (!string.IsNullOrWhiteSpace(trimEnd))
        {
            builder.Append("-to ").Append(trimEnd).Append(' ');
        }

        builder.Append("-vn ");
        var exportCodec = MapAudioExportCodec(ConversionOptionReader.GetString(o, "audio_codec", "mp3"), targetFormat);
        builder.Append("-c:a ").Append(exportCodec).Append(' ');

        if (!IsLosslessAudioTarget(targetFormat, exportCodec))
        {
            var br = ConversionOptionReader.GetString(o, "bitrate", "192");
            builder.Append("-b:a ").Append(br).Append("k ");
        }

        var sampleRate = ConversionOptionReader.GetString(o, "sample_rate", "original");
        if (!string.Equals(sampleRate, "original", StringComparison.OrdinalIgnoreCase))
        {
            builder.Append("-ar ").Append(sampleRate).Append(' ');
        }

        var channels = ConversionOptionReader.GetString(o, "channels", "original");
        if (!string.Equals(channels, "original", StringComparison.OrdinalIgnoreCase))
        {
            builder.Append("-ac ").Append(channels).Append(' ');
        }

        var vol = ConversionOptionReader.GetDouble(o, "volume", 0);
        var normalize = ConversionOptionReader.GetBool(o, "normalize", false);
        var af = new List<string>();
        if (Math.Abs(vol) > 0.001)
        {
            af.Add($"volume={vol.ToString(CultureInfo.InvariantCulture)}dB");
        }

        if (normalize)
        {
            af.Add("loudnorm=I=-14:TP=-1.5:LRA=11");
        }

        if (af.Count > 0)
        {
            builder.Append("-af ").Append(string.Join(',', af)).Append(' ');
        }

        builder.Append("-progress pipe:2 ");
        builder.Append('"').Append(job.OutputPath).Append('"');
        return builder.ToString();
    }

    private static bool IsLosslessAudioTarget(string targetFormat, string ffmpegCodec) =>
        targetFormat is "flac" || ffmpegCodec is "flac" or "alac" or "pcm_s16le" or "pcm_s24le" or "pcm_f32le";

    private static string MapAudioExportCodec(string codecKey, string targetFormat)
    {
        if (targetFormat == "wav")
        {
            return codecKey switch
            {
                "pcm24" => "pcm_s24le",
                "pcm32f" => "pcm_f32le",
                _ => "pcm_s16le",
            };
        }

        return codecKey switch
        {
            "mp3" => "libmp3lame",
            "aac" or "he-aac" or "he-aac-v2" => "aac",
            "vorbis" => "libvorbis",
            "opus" => "libopus",
            "flac" => "flac",
            "alac" => "alac",
            _ => "libmp3lame",
        };
    }

    private static bool ShouldUseYuv420p(string targetFormat, string videoEncoder)
    {
        if (targetFormat is "webm" or "gif")
        {
            return false;
        }

        return videoEncoder.StartsWith("libx264", StringComparison.Ordinal) ||
               videoEncoder.StartsWith("libx265", StringComparison.Ordinal) ||
               videoEncoder.Contains("h264", StringComparison.OrdinalIgnoreCase) ||
               videoEncoder.Contains("hevc", StringComparison.OrdinalIgnoreCase) ||
               videoEncoder.Contains("nvenc", StringComparison.OrdinalIgnoreCase) ||
               videoEncoder.Contains("qsv", StringComparison.OrdinalIgnoreCase) ||
               videoEncoder.Contains("amf", StringComparison.OrdinalIgnoreCase);
    }

    private static void AppendConstantQualityArgs(StringBuilder builder, string encoderMode, string videoEncoder, int q)
    {
        q = Math.Clamp(q, 0, 51);
        if (string.Equals(encoderMode, "software", StringComparison.OrdinalIgnoreCase))
        {
            if (videoEncoder.Contains("265", StringComparison.OrdinalIgnoreCase) || videoEncoder == "libx265")
            {
                builder.Append("-crf ").Append(q).Append(' ');
            }
            else if (videoEncoder == "libaom-av1")
            {
                builder.Append("-crf ").Append(q).Append(" -cpu-used 4 ");
            }
            else
            {
                builder.Append("-crf ").Append(q).Append(' ');
            }

            return;
        }

        if (videoEncoder.Contains("nvenc", StringComparison.OrdinalIgnoreCase))
        {
            builder.Append("-rc vbr -cq ").Append(q).Append(' ');
            return;
        }

        if (videoEncoder.Contains("qsv", StringComparison.OrdinalIgnoreCase))
        {
            var gq = Math.Clamp(51 - q, 18, 51);
            builder.Append("-global_quality ").Append(gq).Append(' ');
            return;
        }

        if (videoEncoder.Contains("amf", StringComparison.OrdinalIgnoreCase))
        {
            builder.Append("-rc cqp -qp_i ").Append(q).Append(" -qp_p ").Append(q).Append(" -qp_b ").Append(q).Append(' ');
            return;
        }

        builder.Append("-crf ").Append(q).Append(' ');
    }

    private static (string Encoder, bool Software) ResolveVideoEncoder(string codecKey, string encoderMode)
    {
        if (string.Equals(encoderMode, "software", StringComparison.OrdinalIgnoreCase))
        {
            return (ConvertVideoCodec(codecKey), true);
        }

        return codecKey.ToLowerInvariant() switch
        {
            "h265" or "hevc" => encoderMode.ToLowerInvariant() switch
            {
                "nvenc" => ("hevc_nvenc", false),
                "qsv" => ("hevc_qsv", false),
                "amf" => ("hevc_amf", false),
                _ => ("libx265", true),
            },
            "av1" => encoderMode.Equals("nvenc", StringComparison.OrdinalIgnoreCase)
                ? ("av1_nvenc", false)
                : ("libaom-av1", true),
            "vp9" => ("libvpx-vp9", true),
            "vp8" => ("libvpx", true),
            _ => encoderMode.ToLowerInvariant() switch
            {
                "nvenc" => ("h264_nvenc", false),
                "qsv" => ("h264_qsv", false),
                "amf" => ("h264_amf", false),
                _ => ("libx264", true),
            },
        };
    }

    private static string GetVideoFilters(ConversionJob job, IReadOnlyDictionary<string, object?> o, string resolution)
    {
        var filters = new List<string>();

        var deinterlace = ConversionOptionReader.GetString(o, "deinterlace", "none");
        filters.Add(deinterlace switch
        {
            "yadif" => "yadif=1:-1:0",
            "bwdif" => "bwdif=1:-1:0",
            _ => string.Empty,
        });

        var rotate = ConversionOptionReader.GetString(o, "rotate", "none");
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

        var speed = ConversionOptionReader.GetDouble(o, "speed", 1);
        if (Math.Abs(speed - 1) > 0.001)
        {
            var pts = (1 / speed).ToString("0.###", CultureInfo.InvariantCulture);
            filters.Add($"setpts={pts}*PTS");
        }

        var subPath = ConversionOptionReader.GetString(o, "subtitle_path");
        var subFilter = BuildSubtitleBurnInFilter(subPath);
        if (!string.IsNullOrEmpty(subFilter))
        {
            filters.Add(subFilter);
        }

        return string.Join(",", filters.Where(static value => !string.IsNullOrWhiteSpace(value)));
    }

    private static string? BuildSubtitleBurnInFilter(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return null;
        }

        try
        {
            var full = Path.GetFullPath(path).Replace('\\', '/');
            var escaped = full.Replace("'", "'\\''", StringComparison.Ordinal);
            if (escaped.Length >= 2 && escaped[1] == ':')
            {
                escaped = escaped[..1] + "\\:" + escaped[2..];
            }

            return $"subtitles='{escaped}'";
        }
        catch
        {
            return null;
        }
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
