using System.Runtime.InteropServices;
using Aiursoft.AiurObserver.Command;
using Aiursoft.AiurObserver.DefaultConsumers;
using Microsoft.Extensions.Logging;

namespace Aiursoft.AiurObserver.Tests;

[TestClass]
public class CommandTest
{
    private readonly string _testCommand =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "-n 2 127.0.0.1" : "-n -c 2 127.0.0.1";

    private readonly string _cancelCommand =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "-n 10 127.0.0.1" : "-n -c 10 127.0.0.1";

    [TestMethod]
    public async Task TestLongRunningCommand()
    {
        var logger = LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<LongCommandRunner>();
        var runner = new LongCommandRunner(logger);
        
        var counter = new MessageCounter<string>();
        var stage = new MessageStageLast<string>();
        runner.Output.Subscribe(counter);
        runner.Output.Subscribe(stage);
        
        await runner.Run("ping", _testCommand, Environment.CurrentDirectory);
        
        Assert.IsGreaterThan(0, counter.Count);
        Assert.IsTrue(stage.Stage?.Contains("ms"));
    }
    
    [TestMethod]
    public async Task TestCancelCommand()
    {
        var logger = LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<LongCommandRunner>();
        var runner = new LongCommandRunner(logger);
        
        var counter = new MessageCounter<string>();
        var stage = new MessageStageLast<string>();
        runner.Output.Subscribe(counter);
        runner.Output.Subscribe(stage);
        
        using var cancelToken = new CancellationTokenSource();
        var task = runner.Run("ping", _cancelCommand, Environment.CurrentDirectory, cancelToken.Token);
        await Task.Delay(5000, cancelToken.Token);
        Assert.IsFalse(task.IsCompleted);
        cancelToken.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => task);
        
        Assert.IsGreaterThan(0, counter.Count);
        Assert.IsNotNull(stage.Stage);
    }
}