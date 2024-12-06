using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Aiursoft.AiurObserver.Command;

/// <summary>
/// A service that runs a long running command and monitor its output.
/// </summary>
public class LongCommandRunner
{
    private readonly ILogger<LongCommandRunner> _logger;

    /// <summary>
    /// Initializes a new instance of the LongCommandRunner class.
    /// </summary>
    /// <param name="logger">The logger to be used for logging.</param>
    public LongCommandRunner(ILogger<LongCommandRunner> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Represents an asynchronous observable stream of string outputs.
    /// </summary>
    public AsyncObservable<string> Output { get; } = new();

    /// <summary>
    /// Gets the async observable for error messages.
    /// </summary>
    /// <remarks>
    /// The Error property provides an async observable that emits error messages of type <see cref="string"/>.
    /// Developers can subscribe to this property to receive error messages asynchronously.
    /// </remarks>
    /// <value>
    /// An async observable that emits error messages of type <see cref="string"/>.
    /// </value>
    public AsyncObservable<string> Error { get; } = new();

    /// <summary>
    /// Asynchronously executes a binary file with arguments in a specified path and monitors its output and error streams.
    /// </summary>
    /// <param name="bin">The path to the binary file.</param>
    /// <param name="arg">The arguments to pass to the binary file.</param>
    /// <param name="path">The path where the output and error streams will be monitored.</param>
    /// <param name="token">A <see cref="CancellationToken"/> to cancel the operation (optional).</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous execution.</returns>
    public async Task Run(
        string bin, 
        string arg, 
        string path,
        CancellationToken token = default)
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = bin,
                Arguments = arg,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = path,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            }
        };
        process.Start();

        _logger.LogTrace("Starting monitor process: {ProcessName} {ProcessArgs}", bin, arg);
        
        // await twice, in case the task throws an exception.
        await await Task.WhenAny(
            // Use Task.Run here. Because calling `process.StandardError.EndOfStream` will block the thread.
            Task.Run(MonitorOutputTask, token), Task.Run(MonitorErrorTask, token), process.WaitForExitAsync(token)
        );
        _logger.LogWarning("The monitor WhenAny task has exited: {ProcessName} {ProcessArgs}", bin, arg);

        // In case the program exit:
        Output.RemoveAllListeners();
        Error.RemoveAllListeners();

        if (!process.HasExited)
        {
            process.Kill();
        }

        return;

        async Task MonitorOutputTask()
        {
            while (!process.StandardOutput.EndOfStream)
            {
                if (token.IsCancellationRequested)
                {
                    process.Kill();
                    return;
                }
                
                await Output.BroadcastAsync(await process.StandardOutput.ReadLineAsync(token) ?? string.Empty);
            }
        }
        
        async Task MonitorErrorTask()
        {
            while (!process.StandardError.EndOfStream)
            {
                if (token.IsCancellationRequested)
                {
                    process.Kill();
                    return;
                }
                
                await Error.BroadcastAsync(await process.StandardError.ReadLineAsync(token) ?? string.Empty);
            }
        }
    }
}
