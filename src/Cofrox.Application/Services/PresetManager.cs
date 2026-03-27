using System.Text.Json;
using Cofrox.Application.Interfaces;
using Cofrox.Application.Models;
using Cofrox.Domain.Interfaces;

namespace Cofrox.Application.Services;

public sealed class PresetManager(ISettingsRepository settingsRepository) : IPresetManager
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly IReadOnlyList<ConversionPreset> BuiltInPresets =
    [
        new(
            "youtube-1080p",
            "YouTube 1080p",
            "Balanced H.264 + AAC output tuned for creator uploads.",
            "mp4",
            ConversionGoal.YouTube,
            new Dictionary<string, string>
            {
                ["resolution"] = "1080p",
                ["video_codec"] = "h264",
                ["audio_mode"] = "reencode",
                ["audio_codec"] = "aac",
                ["audio_bitrate"] = "192",
                ["quality_mode"] = "crf",
                ["quality_value"] = "20",
                ["frame_rate"] = "original",
            },
            true),
        new(
            "mobile-h264",
            "Mobile Friendly",
            "Fast-to-play MP4 tuned for phones and messaging apps.",
            "mp4",
            ConversionGoal.Mobile,
            new Dictionary<string, string>
            {
                ["resolution"] = "720p",
                ["video_codec"] = "h264",
                ["audio_mode"] = "reencode",
                ["audio_codec"] = "aac",
                ["audio_bitrate"] = "128",
                ["quality_mode"] = "crf",
                ["quality_value"] = "24",
                ["frame_rate"] = "30",
            },
            true),
        new(
            "high-quality-hevc",
            "High Quality Archive",
            "HEVC preset for visually strong archive outputs.",
            "mp4",
            ConversionGoal.Quality,
            new Dictionary<string, string>
            {
                ["resolution"] = "original",
                ["video_codec"] = "h265",
                ["audio_mode"] = "keep",
                ["quality_mode"] = "crf",
                ["quality_value"] = "18",
                ["frame_rate"] = "original",
            },
            true),
        new(
            "small-size-hevc",
            "Small Size",
            "Aggressive size-saving preset with acceptable quality for sharing.",
            "mp4",
            ConversionGoal.Size,
            new Dictionary<string, string>
            {
                ["resolution"] = "720p",
                ["video_codec"] = "h265",
                ["audio_mode"] = "reencode",
                ["audio_codec"] = "aac",
                ["audio_bitrate"] = "96",
                ["quality_mode"] = "crf",
                ["quality_value"] = "28",
                ["frame_rate"] = "30",
            },
            true),
    ];

    public async Task<IReadOnlyList<ConversionPreset>> GetAllAsync(CancellationToken cancellationToken)
    {
        var custom = await LoadCustomAsync(cancellationToken).ConfigureAwait(false);
        return BuiltInPresets.Concat(custom).ToArray();
    }

    public Task<IReadOnlyList<ConversionPreset>> GetBuiltInAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(BuiltInPresets);
    }

    public async Task SaveCustomAsync(ConversionPreset preset, CancellationToken cancellationToken)
    {
        var settings = await settingsRepository.LoadAsync(cancellationToken).ConfigureAwait(false);
        var custom = (await LoadCustomAsync(cancellationToken).ConfigureAwait(false)).ToList();
        custom.RemoveAll(existing => string.Equals(existing.Id, preset.Id, StringComparison.OrdinalIgnoreCase));
        custom.Add(preset with { IsBuiltIn = false });

        await settingsRepository.SaveAsync(
            settings with
            {
                CustomPresetsJson = JsonSerializer.Serialize(custom, JsonOptions),
            },
            cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteCustomAsync(string presetId, CancellationToken cancellationToken)
    {
        var settings = await settingsRepository.LoadAsync(cancellationToken).ConfigureAwait(false);
        var custom = (await LoadCustomAsync(cancellationToken).ConfigureAwait(false))
            .Where(preset => !string.Equals(preset.Id, presetId, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        await settingsRepository.SaveAsync(
            settings with
            {
                CustomPresetsJson = JsonSerializer.Serialize(custom, JsonOptions),
            },
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<ConversionPreset>> LoadCustomAsync(CancellationToken cancellationToken)
    {
        var settings = await settingsRepository.LoadAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(settings.CustomPresetsJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<ConversionPreset[]>(settings.CustomPresetsJson, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
