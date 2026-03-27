using Cofrox.Application.Services;
using Cofrox.Domain.Entities;
using Cofrox.Domain.Enums;
using Xunit;

namespace Cofrox.Application.Tests;

public sealed class FFmpegCommandBuilderTests
{
    private readonly FFmpegCommandBuilder _builder = new();

    [Fact]
    public void BuildPlan_AddsLowMemoryThreadLimit_AndNvencEncoder()
    {
        var plan = _builder.BuildPlan(
            CreateVideoJob(),
            new Dictionary<string, object?>
            {
                ["video_codec"] = "h265",
                ["video_encoder"] = "nvenc",
                ["quality_mode"] = "crf",
                ["quality_value"] = 24d,
                ["audio_mode"] = "reencode",
                ["audio_codec"] = "aac",
                ["audio_bitrate"] = "128",
            },
            new SystemProfile { TotalPhysicalMemoryBytes = 2UL * 1024UL * 1024UL * 1024UL });

        Assert.Contains("-threads 2", plan.Arguments, StringComparison.Ordinal);
        Assert.Contains("-c:v hevc_nvenc", plan.Arguments, StringComparison.Ordinal);
        Assert.True(plan.UsesHardwareAcceleration);
    }

    [Fact]
    public void BuildPlan_CreatesAudioOnlyPipeline_WithNormalization()
    {
        var job = new ConversionJob
        {
            Id = Guid.NewGuid().ToString("N"),
            SourceFile = new FileItem
            {
                Id = Guid.NewGuid().ToString("N"),
                FileName = "song.wav",
                SourcePath = @"C:\fixtures\song.wav",
                SourceExtension = "wav",
                SourceFamily = FileFamily.Audio,
                FileSizeBytes = 1024,
            },
            TargetExtension = "mp3",
            OutputPath = @"C:\output\song.mp3",
            Options = new Dictionary<string, object?>(),
        };

        var plan = _builder.BuildPlan(
            job,
            new Dictionary<string, object?>
            {
                ["audio_codec"] = "mp3",
                ["bitrate"] = "192",
                ["normalize"] = true,
            },
            new SystemProfile { TotalPhysicalMemoryBytes = 16UL * 1024UL * 1024UL * 1024UL });

        Assert.Contains("-vn", plan.Arguments, StringComparison.Ordinal);
        Assert.Contains("loudnorm=I=-14:TP=-1.5:LRA=11", plan.Arguments, StringComparison.Ordinal);
        Assert.Contains(plan.Warnings, warning => warning.Contains("normalization", StringComparison.OrdinalIgnoreCase));
    }

    private static ConversionJob CreateVideoJob() =>
        new()
        {
            Id = Guid.NewGuid().ToString("N"),
            SourceFile = new FileItem
            {
                Id = Guid.NewGuid().ToString("N"),
                FileName = "clip.mov",
                SourcePath = @"C:\fixtures\clip.mov",
                SourceExtension = "mov",
                SourceFamily = FileFamily.Video,
                FileSizeBytes = 1024,
            },
            TargetExtension = "mp4",
            OutputPath = @"C:\output\clip.mp4",
            Options = new Dictionary<string, object?>(),
        };
}
