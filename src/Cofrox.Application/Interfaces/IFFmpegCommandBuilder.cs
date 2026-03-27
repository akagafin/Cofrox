using Cofrox.Application.Models;
using Cofrox.Domain.Entities;

namespace Cofrox.Application.Interfaces;

public interface IFFmpegCommandBuilder
{
    FFmpegCommandPlan BuildPlan(
        ConversionJob job,
        IReadOnlyDictionary<string, object?> options,
        SystemProfile profile);
}
