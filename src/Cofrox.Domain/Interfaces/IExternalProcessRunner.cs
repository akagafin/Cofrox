using Cofrox.Domain.ValueObjects;

namespace Cofrox.Domain.Interfaces;

public interface IExternalProcessRunner
{
    Task<ProcessExecutionResult> RunAsync(
        ProcessExecutionRequest request,
        Func<string, double?>? progressParser,
        IProgress<double>? progress,
        CancellationToken cancellationToken);
}
