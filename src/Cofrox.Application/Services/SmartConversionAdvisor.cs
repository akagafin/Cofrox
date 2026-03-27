using Cofrox.Application.Interfaces;
using Cofrox.Application.Models;
using Cofrox.Domain.Entities;
using Cofrox.Domain.Enums;

namespace Cofrox.Application.Services;

public sealed class SmartConversionAdvisor : ISmartConversionAdvisor
{
    public ConversionRecommendation Recommend(
        FileItem sourceFile,
        string targetExtension,
        ConversionGoal goal,
        IReadOnlyDictionary<string, object?> currentOptions)
    {
        var target = targetExtension.TrimStart('.').ToLowerInvariant();
        var warnings = new List<string>();
        var recommended = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        switch (sourceFile.SourceFamily)
        {
            case FileFamily.Video:
            case FileFamily.Audio:
                ApplyMediaDefaults(sourceFile, target, goal, recommended, warnings);
                break;
            case FileFamily.Image:
            case FileFamily.RawImage:
                ApplyImageDefaults(target, goal, recommended);
                break;
            case FileFamily.Document:
            case FileFamily.Pdf:
            case FileFamily.Spreadsheet:
                ApplyDocumentDefaults(target, recommended);
                break;
        }

        foreach (var pair in currentOptions)
        {
            if (pair.Value is not null)
            {
                recommended[pair.Key] = pair.Value;
            }
        }

        var ratio = goal switch
        {
            ConversionGoal.Size => 0.35,
            ConversionGoal.Mobile => 0.45,
            ConversionGoal.YouTube => 0.55,
            ConversionGoal.Quality => 0.85,
            _ => 0.65,
        };

        var presetId = goal switch
        {
            ConversionGoal.YouTube => "youtube-1080p",
            ConversionGoal.Mobile => "mobile-h264",
            ConversionGoal.Quality => "high-quality-hevc",
            ConversionGoal.Size => "small-size-hevc",
            _ => "balanced-auto",
        };

        if (sourceFile.FileSizeBytes >= 10L * 1024 * 1024 * 1024)
        {
            warnings.Add("Large file detected (>10GB). Keep temp cleanup enabled and avoid aggressive parallelism.");
        }

        return new ConversionRecommendation(target, goal, presetId, recommended, warnings, ratio);
    }

    private static void ApplyMediaDefaults(
        FileItem sourceFile,
        string targetExtension,
        ConversionGoal goal,
        IDictionary<string, object?> options,
        ICollection<string> warnings)
    {
        if (targetExtension is "mp3" or "flac" or "aac" or "wav" or "ogg" or "m4a" or "opus")
        {
            options["bitrate"] = goal switch
            {
                ConversionGoal.Size => "128",
                ConversionGoal.Mobile => "128",
                ConversionGoal.Quality => "320",
                _ => "192",
            };
            options["sample_rate"] = "original";
            options["channels"] = "original";
            return;
        }

        options["video_codec"] = goal switch
        {
            ConversionGoal.YouTube => "h264",
            ConversionGoal.Mobile => "h264",
            ConversionGoal.Size => "h265",
            ConversionGoal.Quality => "h265",
            _ => "h264",
        };
        options["audio_mode"] = "reencode";
        options["audio_codec"] = "aac";
        options["audio_bitrate"] = goal switch
        {
            ConversionGoal.Size => "96",
            ConversionGoal.Mobile => "128",
            _ => "192",
        };
        options["quality_mode"] = "crf";
        options["quality_value"] = goal switch
        {
            ConversionGoal.Quality => 18d,
            ConversionGoal.Size => 28d,
            ConversionGoal.Mobile => 24d,
            ConversionGoal.YouTube => 20d,
            _ => 23d,
        };
        options["resolution"] = goal switch
        {
            ConversionGoal.Mobile => "720p",
            ConversionGoal.Size when sourceFile.SourceFamily == FileFamily.Video => "720p",
            _ => "original",
        };
        options["frame_rate"] = goal == ConversionGoal.Mobile ? "30" : "original";

        if (targetExtension == "webm")
        {
            options["video_codec"] = goal == ConversionGoal.Quality ? "av1" : "vp9";
            warnings.Add("WebM output is best paired with VP9 or AV1 video.");
        }
    }

    private static void ApplyImageDefaults(string targetExtension, ConversionGoal goal, IDictionary<string, object?> options)
    {
        options["quality"] = targetExtension == "png" ? 6d : goal == ConversionGoal.Quality ? 92d : 85d;
        options["resize_mode"] = "none";
        options["strip_metadata"] = goal is ConversionGoal.Size or ConversionGoal.Mobile;
        options["dpi"] = goal == ConversionGoal.Quality ? "300" : "72";
    }

    private static void ApplyDocumentDefaults(string targetExtension, IDictionary<string, object?> options)
    {
        options["encoding"] = "utf-8";
        options["include_images"] = true;
        if (targetExtension == "pdf")
        {
            options["pdf_version"] = "1.7";
            options["compress_pdf"] = true;
        }
    }
}
