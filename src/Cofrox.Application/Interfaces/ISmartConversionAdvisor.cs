using Cofrox.Application.Models;
using Cofrox.Domain.Entities;

namespace Cofrox.Application.Interfaces;

public interface ISmartConversionAdvisor
{
    ConversionRecommendation Recommend(
        FileItem sourceFile,
        string targetExtension,
        ConversionGoal goal,
        IReadOnlyDictionary<string, object?> currentOptions);
}
