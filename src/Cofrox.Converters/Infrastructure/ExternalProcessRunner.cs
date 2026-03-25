using System.Diagnostics;
using System.Text;
using Cofrox.Domain.Interfaces;
using Cofrox.Domain.ValueObjects;

namespace Cofrox.Converters.Infrastructure;

public sealed class ExternalProcessRunner : IExternalProcessRunner
{
    public async Task<ProcessExecutionResult> RunAsync(
        ProcessExecutionRequest request,
        Func<string, double?>? progressParser,
        IProgress<double>? progress,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = request.FileName,
            Arguments = request.Arguments,
            WorkingDirectory = request.WorkingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        process.OutputDataReceived += (_, eventArgs) => HandleLine(eventArgs.Data, stdout, progressParser, progress);
        process.ErrorDataReceived += (_, eventArgs) => HandleLine(eventArgs.Data, stderr, progressParser, progress);

        if (!process.Start())
        {
            throw new InvalidOperationException($"Unable to start process '{request.FileName}'.");
        }

        try
        {
            process.PriorityClass = ProcessPriorityClass.BelowNormal;
        }
        catch (InvalidOperationException)
        {
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var timeoutCts = new CancellationTokenSource(request.Timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        var timedOut = false;
        try
        {
            await process.WaitForExitAsync(linkedCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            timedOut = timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested;
            TryKill(process);

            if (!timedOut)
            {
                throw;
            }
        }

        if (!process.HasExited)
        {
            await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
        }

        return new ProcessExecutionResult
        {
            ExitCode = process.HasExited ? process.ExitCode : -1,
            StandardOutput = stdout.ToString(),
            StandardError = stderr.ToString(),
            TimedOut = timedOut,
        };
    }

    private static void HandleLine(
        string? line,
        StringBuilder builder,
        Func<string, double?>? progressParser,
        IProgress<double>? progress)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return;
        }

        builder.AppendLine(line);
        var progressValue = progressParser?.Invoke(line);
        if (progressValue is not null)
        {
            progress?.Report(Math.Clamp(progressValue.Value, 0, 1));
        }
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
        }
    }
}
