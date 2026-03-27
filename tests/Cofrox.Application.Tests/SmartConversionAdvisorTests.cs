using Cofrox.Application.Models;
using Cofrox.Application.Services;
using Cofrox.Domain.Entities;
using Cofrox.Domain.Enums;
using Xunit;

namespace Cofrox.Application.Tests;

public sealed class SmartConversionAdvisorTests
{
    private readonly SmartConversionAdvisor _advisor = new();

    [Fact]
    public void Recommend_UsesMobileDefaults_ForVideo()
    {
        var recommendation = _advisor.Recommend(
            CreateFile(FileFamily.Video, 512 * 1024 * 1024),
            "mp4",
            ConversionGoal.Mobile,
            new Dictionary<string, object?>());

        Assert.Equal("mobile-h264", recommendation.RecommendedPresetId);
        Assert.Equal("720p", recommendation.RecommendedOptions["resolution"]);
        Assert.Equal("30", recommendation.RecommendedOptions["frame_rate"]);
    }

    [Fact]
    public void Recommend_AddsLargeFileWarning_ForHugeAssets()
    {
        var recommendation = _advisor.Recommend(
            CreateFile(FileFamily.Video, 12L * 1024 * 1024 * 1024),
            "mp4",
            ConversionGoal.Size,
            new Dictionary<string, object?>());

        Assert.Contains(recommendation.Warnings, warning => warning.Contains(">10GB", StringComparison.Ordinal));
    }

    [Fact]
    public void Recommend_ChoosesVp9ForWebM()
    {
        var recommendation = _advisor.Recommend(
            CreateFile(FileFamily.Video, 100 * 1024 * 1024),
            "webm",
            ConversionGoal.Balanced,
            new Dictionary<string, object?>());

        Assert.Equal("vp9", recommendation.RecommendedOptions["video_codec"]);
        Assert.Contains(recommendation.Warnings, warning => warning.Contains("WebM", StringComparison.Ordinal));
    }

    private static FileItem CreateFile(FileFamily family, long size) =>
        new()
        {
            Id = Guid.NewGuid().ToString("N"),
            FileName = "sample.input",
            SourcePath = @"C:\fixtures\sample.input",
            SourceExtension = family == FileFamily.Audio ? "wav" : "mp4",
            SourceFamily = family,
            FileSizeBytes = size,
        };
}
