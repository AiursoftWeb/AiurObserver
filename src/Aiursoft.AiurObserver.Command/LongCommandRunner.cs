using System.Diagnostics;

namespace Aiursoft.AiurObserver.Command;

/// <summary>
/// A service that runs a long running command and monitor its output.
/// </summary>
public class LongCommandRunner
{
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

        await Task.WhenAny(
            MonitorOutputTask(),
            MonitorErrorTask(),
            process.WaitForExitAsync()
        );

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