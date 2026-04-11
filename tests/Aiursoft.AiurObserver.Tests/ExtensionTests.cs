namespace Aiursoft.AiurObserver.Tests;

[TestClass]
public class ExtensionTests
{
    [TestMethod]
    public async Task TestCounterExtension()
    {
        var source = new AsyncObservable<int>();
        var counter = source.Counter();
        
        await source.BroadcastAsync(1);
        await source.BroadcastAsync(2);
        
        Assert.AreEqual(2, counter.Count);
    }

    [TestMethod]
    public async Task TestStageFirstExtension()
    {
        var source = new AsyncObservable<int>();
        var stage = source.StageFirst();
        
        await source.BroadcastAsync(10);
        await source.BroadcastAsync(20);
        
        Assert.AreEqual(10, stage.Stage);
    }

    [TestMethod]
    public async Task TestStageLastExtension()
    {
        var source = new AsyncObservable<int>();
        var stage = source.StageLast();
        
        await source.BroadcastAsync(10);
        await source.BroadcastAsync(20);
        
        Assert.AreEqual(20, stage.Stage);
    }

    [TestMethod]
    public async Task TestStageSpecificExtension()
    {
        var source = new AsyncObservable<int>();
        var stage = source.StageSpecific(1);
        
        await source.BroadcastAsync(10);
        await source.BroadcastAsync(20);
        await source.BroadcastAsync(30);
        
        Assert.AreEqual(20, stage.Stage);
    }

    [TestMethod]
    public async Task TestAdderExtension()
    {
        var source = new AsyncObservable<double>();
        var adder = source.Adder();
        
        await source.BroadcastAsync(1.5);
        await source.BroadcastAsync(2.5);
        
        Assert.AreEqual(4.0, adder.Sum);
    }

    [TestMethod]
    public async Task TestAverageExtension()
    {
        var source = new AsyncObservable<int>();
        var average = source.Average();
        
        await source.BroadcastAsync(10);
        await source.BroadcastAsync(20);
        
        var (sum, count) = average.Average();
        Assert.AreEqual(30, sum);
        Assert.AreEqual(2, count);
    }

    [TestMethod]
    public async Task TestAverageRecentExtension()
    {
        var source = new AsyncObservable<int>();
        var average = source.AverageRecent(2);
        
        await source.BroadcastAsync(10);
        await source.BroadcastAsync(20);
        await source.BroadcastAsync(30);
        
        var (sum, count) = average.Average();
        Assert.AreEqual(50, sum); // 20 + 30
        Assert.AreEqual(2, count);
    }

    [TestMethod]
    public async Task TestUsingSubscription()
    {
        var source = new AsyncObservable<int>();
        var count = 0;
        {
            using var sub = source.Subscribe(_ => 
            {
                count++;
                return Task.CompletedTask;
            });
            await source.BroadcastAsync(1);
        }
        
        Assert.AreEqual(1, count);
        
        // This should not increase count because sub is disposed
        await source.BroadcastAsync(2);
        Assert.AreEqual(1, count);
    }
}
