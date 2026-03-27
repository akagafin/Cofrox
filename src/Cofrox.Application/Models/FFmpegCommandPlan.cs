namespace Cofrox.Application.Models;

public sealed record FFmpegCommandPlan(
    string Arguments,
    string VideoEncoder,
    bool UsesHardwareAcceleration,
    IReadOnlyList<string> Warnings,
    double EstimatedCompressionRatio);
