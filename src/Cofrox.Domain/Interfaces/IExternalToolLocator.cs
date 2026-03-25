namespace Cofrox.Domain.Interfaces;

public interface IExternalToolLocator
{
    string? Resolve(string logicalName);
}
