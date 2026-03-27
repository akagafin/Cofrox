using Cofrox.Application.Models;

namespace Cofrox.Application.Interfaces;

public interface IPresetManager
{
    Task<IReadOnlyList<ConversionPreset>> GetAllAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<ConversionPreset>> GetBuiltInAsync(CancellationToken cancellationToken);

    Task SaveCustomAsync(ConversionPreset preset, CancellationToken cancellationToken);

    Task DeleteCustomAsync(string presetId, CancellationToken cancellationToken);
}
