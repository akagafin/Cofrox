using System.Globalization;
using System.Text;
using Cofrox.Application.Interfaces;
using Cofrox.Application.Models;
using Cofrox.Domain.Entities;

namespace Cofrox.Application.Services;

public sealed class FFmpegCommandBuilder : IFFmpegCommandBuilder
{
    public FFmpegCommandPlan BuildPlan(ConversionJob job, IReadOnlyDictionary<string, object?> options, SystemProfile profile)
    {
        var targetFormat = Path.GetExtension(job.OutputPath).TrimStart('.').ToLowerInvariant();
        return IsAudioOnlyContainer(targetFormat)
            ? BuildAudioPlan(job, options, profile, targetFormat)
            : BuildVideoPlan(job, options, profile, targetFormat);
    }

    private static FFmpegCommandPlan BuildVideoPlan(ConversionJob job, IReadOnlyDictionary<string, object?> options, SystemProfile profile, string targetFormat)
    {
        var warnings = new List<string>();
        var builder = new StringBuilder("-y ");
        builder.Append("-i ").Append('"').Append(job.SourceFile.SourcePath).Append("\" ");

        if (profile.IsLowMemoryDevice)
        {
            builder.Append("-threads 2 ");
            warnings.Add("Low-memory profile detected; limiting FFmpeg threads to 2.");
        }

        AppendTrim(builder, options);

        var filters = BuildVideoFilters(options);
        if (!string.IsNullOrWhiteSpace(filters))
        {
            builder.Append("-vf ").Append('"').Append(filters).Append("\" ");
        }

        var audioMode = GetString(options, "audio_mode", "keep");
        if (audioMode == "remove")
        {
            builder.Append("-an ");
        }
        else if (audioMode == "reencode")
        {
            builder.Append("-c:a ").Append(ConvertAudioCodec(GetString(options, "audio_codec", "aac"))).Append(' ');
            builder.Append("-b:a ").Append(GetString(options, "audio_bitrate", "128")).Append("k ");
        }
        else
        {
            builder.Append("-c:a copy ");
        }

        var codecKey = GetString(options, "video_codec", "h264");
        var encoderMode = GetString(options, "video_encoder", "software");
        var speed = GetString(options, "encoding_speed", "medium");
        if (profile.IsLowMemoryDevice && encoderMode == "software")
        {
            speed = "ultrafast";
        }

        var (videoEncoder, usesSoftwareCodec) = ResolveVideoEncoder(codecKey, encoderMode);
        builder.Append("-c:v ").Append(videoEncoder).Append(' ');

        if (usesSoftwareCodec)
        {
            builder.Append("-preset ").Append(speed).Append(' ');
        }
        else if (encoderMode == "nvenc")
        {
            builder.Append("-preset p4 ");
        }

        var qualityMode = GetString(options, "quality_mode", "crf");
        var qualityValue = (int)GetDouble(options, "quality_value", 23);
        var targetBitrate = (int)GetDouble(options, "target_bitrate", 2500);
        if (qualityMode == "bitrate")
        {
            builder.Append("-b:v ").Append(targetBitrate).Append("k ");
        }
        else
        {
            AppendConstantQualityArgs(builder, encoderMode, videoEncoder, qualityValue);
        }

        if (ShouldUseYuv420p(targetFormat, videoEncoder))
        {
            builder.Append("-pix_fmt yuv420p ");
        }

        var fps = GetString(options, "frame_rate", "original");
        if (fps != "original" && double.TryParse(fps, NumberStyles.Float, CultureInfo.InvariantCulture, out var fpsValue))
        {
            builder.Append(CultureInfo.InvariantCulture, $"-r {fpsValue} ");
        }

        if (videoEncoder.Contains("av1", StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add("AV1 delivers strong compression but encodes slower than H.264/H.265.");
        }

        builder.Append("-progress pipe:2 ");
        builder.Append('"').Append(job.OutputPath).Append('"');

        var ratio = qualityMode == "bitrate"
            ? 0.5
            : Math.Clamp((qualityValue + 5) / 40d, 0.25, 0.95);

        return new FFmpegCommandPlan(builder.ToString(), videoEncoder, !usesSoftwareCodec, warnings, ratio);
    }

    private static FFmpegCommandPlan BuildAudioPlan(ConversionJob job, IReadOnlyDictionary<string, object?> options, SystemProfile profile, string targetFormat)
    {
        var warnings = new List<string>();
        var builder = new StringBuilder("-y ");
        builder.Append("-i ").Append('"').Append(job.SourceFile.SourcePath).Append("\" ");

        if (profile.IsLowMemoryDevice)
        {
            builder.Append("-threads 2 ");
        }

        AppendTrim(builder, options);
        builder.Append("-vn ");

        var codec = MapAudioExportCodec(GetString(options, "audio_codec", "mp3"), targetFormat);
        builder.Append("-c:a ").Append(codec).Append(' ');
        if (!IsLosslessAudioTarget(targetFormat, codec))
        {
            builder.Append("-b:a ").Append(GetString(options, "bitrate", "192")).Append("k ");
        }

        var sampleRate = GetString(options, "sample_rate", "original");
        if (sampleRate != "original")
        {
            builder.Append("-ar ").Append(sampleRate).Append(' ');
        }

        var channels = GetString(options, "channels", "original");
        if (channels != "original")
        {
            builder.Append("-ac ").Append(channels).Append(' ');
        }

        var audioFilters = new List<string>();
        var volume = GetDouble(options, "volume", 0);
        if (Math.Abs(volume) > 0.001d)
        {
            audioFilters.Add($"volume={volume.ToString("0.###", CultureInfo.InvariantCulture)}dB");
        }

        if (GetBool(options, "normalize", false))
        {
            audioFilters.Add("loudnorm=I=-14:TP=-1.5:LRA=11");
            warnings.Add("Audio normalization is enabled and may increase processing time.");
        }

        if (audioFilters.Count > 0)
        {
            builder.Append("-af ").Append('"').Append(string.Join(',', audioFilters)).Append("\" ");
        }

        builder.Append("-progress pipe:2 ");
        builder.Append('"').Append(job.OutputPath).Append('"');
        return new FFmpegCommandPlan(builder.ToString(), codec, false, warnings, IsLosslessAudioTarget(targetFormat, codec) ? 1.0 : 0.6);
    }

    private static void AppendTrim(StringBuilder builder, IReadOnlyDictionary<string, object?> options)
    {
        var trimStart = GetString(options, "trim_start");
        var trimEnd = GetString(options, "trim_end");
        if (!string.IsNullOrWhiteSpace(trimStart))
        {
            builder.Append("-ss ").Append(trimStart).Append(' ');
        }

        if (!string.IsNullOrWhiteSpace(trimEnd))
        {
            builder.Append("-to ").Append(trimEnd).Append(' ');
        }
    }

    private static string BuildVideoFilters(IReadOnlyDictionary<string, object?> options)
    {
        var filters = new List<string>();
        filters.Add(GetString(options, "deinterlace", "none") switch
        {
            "yadif" => "yadif=1:-1:0",
            "bwdif" => "bwdif=1:-1:0",
            _ => string.Empty,
        });
        filters.Add(GetString(options, "rotate", "none") switch
        {
            "cw90" => "transpose=1",
            "ccw90" => "transpose=2",
            "180" => "transpose=2,transpose=2",
            "flip_h" => "hflip",
            "flip_v" => "vflip",
            _ => string.Empty,
        });
        filters.Add(GetString(options, "resolution", "original") switch
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

        var speed = GetDouble(options, "speed", 1);
        if (Math.Abs(speed - 1d) > 0.001d)
        {
            filters.Add($"setpts={(1d / speed).ToString("0.###", CultureInfo.InvariantCulture)}*PTS");
        }

        var subtitlePath = GetString(options, "subtitle_path");
        if (!string.IsNullOrWhiteSpace(subtitlePath) && File.Exists(subtitlePath))
        {
            var escaped = Path.GetFullPath(subtitlePath).Replace('\\', '/').Replace(":", "\\:", StringComparison.Ordinal);
            filters.Add($"subtitles='{escaped}'");
        }

        return string.Join(",", filters.Where(static value => !string.IsNullOrWhiteSpace(value)));
    }

    private static (string Encoder, bool Software) ResolveVideoEncoder(string codecKey, string encoderMode)
    {
        if (encoderMode == "software")
        {
            return (ConvertVideoCodec(codecKey), true);
        }

        return codecKey.ToLowerInvariant() switch
        {
            "h265" or "hevc" => encoderMode switch
            {
                "nvenc" => ("hevc_nvenc", false),
                "qsv" => ("hevc_qsv", false),
                "amf" => ("hevc_amf", false),
                _ => ("libx265", true),
            },
            "av1" => encoderMode == "nvenc" ? ("av1_nvenc", false) : ("libaom-av1", true),
            "vp9" => ("libvpx-vp9", true),
            "vp8" => ("libvpx", true),
            _ => encoderMode switch
            {
                "nvenc" => ("h264_nvenc", false),
                "qsv" => ("h264_qsv", false),
                "amf" => ("h264_amf", false),
                _ => ("libx264", true),
            },
        };
    }

    private static bool ShouldUseYuv420p(string targetFormat, string videoEncoder) =>
        targetFormat is not "webm" and not "gif" &&
        (videoEncoder.Contains("264", StringComparison.OrdinalIgnoreCase) ||
         videoEncoder.Contains("265", StringComparison.OrdinalIgnoreCase) ||
         videoEncoder.Contains("hevc", StringComparison.OrdinalIgnoreCase) ||
         videoEncoder.Contains("nvenc", StringComparison.OrdinalIgnoreCase) ||
         videoEncoder.Contains("qsv", StringComparison.OrdinalIgnoreCase) ||
         videoEncoder.Contains("amf", StringComparison.OrdinalIgnoreCase));

    private static void AppendConstantQualityArgs(StringBuilder builder, string encoderMode, string videoEncoder, int quality)
    {
        quality = Math.Clamp(quality, 0, 51);
        if (encoderMode == "software")
        {
            builder.Append("-crf ").Append(quality).Append(' ');
            if (videoEncoder == "libaom-av1")
            {
                builder.Append("-cpu-used 4 ");
            }

            return;
        }

        if (videoEncoder.Contains("nvenc", StringComparison.OrdinalIgnoreCase))
        {
            builder.Append("-rc vbr -cq ").Append(quality).Append(' ');
            return;
        }

        if (videoEncoder.Contains("qsv", StringComparison.OrdinalIgnoreCase))
        {
            builder.Append("-global_quality ").Append(Math.Clamp(51 - quality, 18, 51)).Append(' ');
            return;
        }

        if (videoEncoder.Contains("amf", StringComparison.OrdinalIgnoreCase))
        {
            builder.Append("-rc cqp -qp_i ").Append(quality).Append(" -qp_p ").Append(quality).Append(" -qp_b ").Append(quality).Append(' ');
            return;
        }

        builder.Append("-crf ").Append(quality).Append(' ');
    }

    private static bool IsAudioOnlyContainer(string targetFormat) =>
        targetFormat is "mp3" or "aac" or "m4a" or "opus" or "ogg" or "flac" or "wav";

    private static bool IsLosslessAudioTarget(string targetFormat, string codec) =>
        targetFormat == "flac" || codec is "flac" or "alac" or "pcm_s16le" or "pcm_s24le" or "pcm_f32le";

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

    private static string GetString(IReadOnlyDictionary<string, object?> options, string key, string fallback = "")
    {
        if (!options.TryGetValue(key, out var value) || value is null)
        {
            return fallback;
        }

        return value.ToString() ?? fallback;
    }

    private static double GetDouble(IReadOnlyDictionary<string, object?> options, string key, double fallback = 0)
    {
        if (!options.TryGetValue(key, out var value) || value is null)
        {
            return fallback;
        }

        return value switch
        {
            double number => number,
            float number => number,
            int number => number,
            long number => number,
            decimal number => (double)number,
            _ when double.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => fallback,
        };
    }

    private static bool GetBool(IReadOnlyDictionary<string, object?> options, string key, bool fallback = false)
    {
        if (!options.TryGetValue(key, out var value) || value is null)
        {
            return fallback;
        }

        return value switch
        {
            bool boolean => boolean,
            _ when bool.TryParse(value.ToString(), out var parsed) => parsed,
            _ => fallback,
        };
    }
}
