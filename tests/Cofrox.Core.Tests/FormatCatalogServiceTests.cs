using Cofrox.Core.Services;
using Xunit;

namespace Cofrox.Core.Tests;

public sealed class FormatCatalogServiceTests
{
    private readonly FormatCatalogService _service = new();

    [Fact]
    public void GetTargets_ReturnsConfiguredTargets_ForMp4()
    {
        var targets = _service.GetTargets("mp4");

        Assert.Contains(targets, static item => item.Extension == "mkv");
        Assert.Contains(targets, static item => item.Extension == "gif");
        Assert.Contains(targets, static item => item.Extension == "mp3");
    }

    [Fact]
    public void GetOptions_ReturnsVideoOptions_ForMp4Target()
    {
        var options = _service.GetOptions("mov", "mp4");

        Assert.Contains(options, static item => item.Key == "video_codec");
        Assert.Contains(options, static item => item.Key == "video_preset");
        Assert.Contains(options, static item => item.Key == "video_encoder");
        Assert.Contains(options, static item => item.Key == "deinterlace");
        Assert.Contains(options, static item => item.Key == "speed");
    }
}
