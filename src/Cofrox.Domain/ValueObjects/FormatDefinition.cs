using Cofrox.Domain.Enums;

namespace Cofrox.Domain.ValueObjects;

public sealed record FormatDefinition(
    string Extension,
    string DisplayName,
    string Category,
    FileFamily Family,
    string Description,
    bool IsLossless = false);
