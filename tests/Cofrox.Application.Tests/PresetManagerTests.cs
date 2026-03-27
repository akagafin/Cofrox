using Cofrox.Application.Models;
using Cofrox.Application.Services;
using Cofrox.Application.Tests.Support;
using Xunit;

namespace Cofrox.Application.Tests;

public sealed class PresetManagerTests
{
    [Fact]
    public async Task SaveCustomAsync_PersistsCustomPreset()
    {
        var repository = new InMemorySettingsRepository();
        var manager = new PresetManager(repository);

        await manager.SaveCustomAsync(
            new ConversionPreset(
                "custom-youtube",
                "Custom YouTube",
                "User tuned preset.",
                "mp4",
                ConversionGoal.YouTube,
                new Dictionary<string, string> { ["video_codec"] = "h264" },
                false),
            CancellationToken.None);

        var presets = await manager.GetAllAsync(CancellationToken.None);

        Assert.Contains(presets, preset => preset.Id == "custom-youtube" && !preset.IsBuiltIn);
    }

    [Fact]
    public async Task DeleteCustomAsync_RemovesPreset()
    {
        var repository = new InMemorySettingsRepository();
        var manager = new PresetManager(repository);
        var preset = new ConversionPreset(
            "custom-mobile",
            "Custom Mobile",
            "User tuned preset.",
            "mp4",
            ConversionGoal.Mobile,
            new Dictionary<string, string> { ["resolution"] = "720p" },
            false);

        await manager.SaveCustomAsync(preset, CancellationToken.None);
        await manager.DeleteCustomAsync("custom-mobile", CancellationToken.None);

        var presets = await manager.GetAllAsync(CancellationToken.None);

        Assert.DoesNotContain(presets, existing => existing.Id == "custom-mobile");
    }
}
