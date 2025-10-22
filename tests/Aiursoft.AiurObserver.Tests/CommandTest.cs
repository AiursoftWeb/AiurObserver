using System.Runtime.InteropServices;
using Aiursoft.AiurObserver.Command;
using Aiursoft.AiurObserver.DefaultConsumers;
using Microsoft.Extensions.Logging;

namespace Aiursoft.AiurObserver.Tests;

[TestClass]
public class CommandTest
{
    private readonly string _testCommand =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "-n 2 www.aiursoft.cn" : "-c 2 www.aiursoft.cn";

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
        
        var cancelToken = new CancellationTokenSource();
        var task = runner.Run("ping", _testCommand, Environment.CurrentDirectory, cancelToken.Token);
        await Task.Delay(5000, cancelToken.Token);
        cancelToken.Cancel();
        try
        {
            await task;
        }
        catch (Exception e)
        {
            Assert.IsTrue(e is TaskCanceledException);
        }
        
        Assert.IsGreaterThan(0, counter.Count);
        Assert.IsNotNull(stage.Stage);
    }
}