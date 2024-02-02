using System.Runtime.InteropServices;
using Aiursoft.AiurObserver.Command;
using Aiursoft.AiurObserver.DefaultConsumers;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.AiurObserver.Tests;

[TestClass]
public class CommandTest
{
    private readonly string _testCommand =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "-n 2 baidu.com" : "-c 2 baidu.com";

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
        
        Assert.IsTrue(counter.Count > 0);
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
        await Task.Delay(2000, cancelToken.Token);
        cancelToken.Cancel();
        try
        {
            await task;
        }
        catch (Exception e)
        {
            Assert.IsTrue(e is TaskCanceledException);
        }
        
        Assert.IsTrue(counter.Count > 0);
        Assert.IsTrue(stage.Stage != null);
    }
}