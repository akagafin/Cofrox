using Cofrox.Domain.ValueObjects;

namespace Cofrox.Domain.Interfaces;

public interface IFormatCatalog
{
    IReadOnlyList<FormatDefinition> GetAllFormats();

    FormatDefinition GetByExtension(string extension);

    IReadOnlyList<FormatDefinition> GetTargets(string sourceExtension);

    IReadOnlyList<FormatOptionDefinition> GetOptions(string sourceExtension, string targetExtension);
}
