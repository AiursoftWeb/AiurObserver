﻿using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Aiursoft.AiurObserver.Command;

/// <summary>
/// A service that runs a long running command and monitor its output.
/// </summary>
public class LongCommandRunner
{
    private readonly ILogger<LongCommandRunner> _logger;

    public LongCommandRunner(ILogger<LongCommandRunner> logger)
    {
        _logger = logger;
    }
    
    public AsyncObservable<string> Output { get; } = new();
    
    public AsyncObservable<string> Error { get; } = new();
    
    private bool ShouldStop { get; set; }
    
    public void Stop()
    {
        ShouldStop = true;
    }
    
    public async Task Run(
        string bin, 
        string arg, 
        string path)
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
                WorkingDirectory = Path.GetTempPath(),
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
            Task.Run(MonitorOutputTask), Task.Run(MonitorErrorTask), process.WaitForExitAsync()
        );
        _logger.LogWarning("The monitor WhenAny task has exited: {ProcessName} {ProcessArgs}", bin, arg);

        // In case the program exit:
        Output.RemoveAllListeners();
        Error.RemoveAllListeners();

        if (!process.HasExited)
        {
            process.Kill();
        }

        if (!ShouldStop)
        {
            throw new InvalidOperationException("The process has exited, while the monitor is still running!");
        }

        return;

        async Task MonitorOutputTask()
        {
            while (!process.StandardOutput.EndOfStream && !ShouldStop)
            {
                await Output.BroadcastAsync(await process.StandardOutput.ReadLineAsync() ?? string.Empty);
            }
        }
        
        async Task MonitorErrorTask()
        {
            while (!process.StandardError.EndOfStream && !ShouldStop)
            {
                await Error.BroadcastAsync(await process.StandardError.ReadLineAsync() ?? string.Empty);
            }
        }
    }
}
