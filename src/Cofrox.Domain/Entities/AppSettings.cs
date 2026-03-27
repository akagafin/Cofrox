using Cofrox.Domain.Enums;

namespace Cofrox.Domain.Entities;

public sealed record AppSettings
{
    public static AppSettings Default => new();

    public string OutputFolderPath { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Downloads",
        "Cofrox");

    public string DefaultVideoQuality { get; init; } = "High";

    public int DefaultImageQuality { get; init; } = 85;

    public int MaxParallelConversions { get; init; } = 0;

    public bool AutoDeleteTempFiles { get; init; } = true;

    public AppThemeMode ThemeMode { get; init; } = AppThemeMode.System;

    public bool SaveHistory { get; init; } = true;

    public string? LibreOfficePath { get; init; }

    public string CustomPresetsJson { get; init; } = "[]";

    public string PersistentQueueStateJson { get; init; } = "[]";
}
