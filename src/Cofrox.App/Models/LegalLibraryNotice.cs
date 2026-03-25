namespace Cofrox.App.Models;

public sealed class LegalLibraryNotice
{
    public required string Name { get; init; }

    public required string Version { get; init; }

    public required string LicenseType { get; init; }

    public required string Copyright { get; init; }

    public required string LicenseText { get; init; }

    public string? Note { get; init; }

    public LegalBadgeKind BadgeKind { get; init; }

    public bool HasNote => !string.IsNullOrWhiteSpace(Note);
}
