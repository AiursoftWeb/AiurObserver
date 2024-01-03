using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.AiurObserver.Tests;

[TestClass]
public class IntegrationTests
{
    [TestMethod]
    public async Task BroadcastListenTest()
    {
        var totalMessages = 0;
        var asyncObservable = new AsyncObservable<int>();
        asyncObservable.Subscribe(_ =>
        {
            totalMessages++;
            return Task.CompletedTask;
        });
        for (var i = 0; i < 10; i++)
        {
            await asyncObservable.BroadcastAsync(2333);
        }

        Assert.AreEqual(10, totalMessages);
    }
    
    [TestMethod]
    public async Task MultipleBroadcastListenTest()
    {
        var totalMessages = 0;
        var asyncObservable = new AsyncObservable<int>();
        asyncObservable.Subscribe(_ =>
        {
            totalMessages++;
            return Task.CompletedTask;
        });
        asyncObservable.Subscribe(_ =>
        {
            totalMessages++;
            return Task.CompletedTask;
        });
        for (var i = 0; i < 10; i++)
        {
            await asyncObservable.BroadcastAsync(2333);
        }

        Assert.AreEqual(20, totalMessages);
    }
    
    [TestMethod]
    public async Task UnRegisterTest()
    {
        var totalMessages = 0;
        var asyncObservable = new AsyncObservable<int>();
        var subscription = asyncObservable.Subscribe(_ =>
        {
            totalMessages++;
            return Task.CompletedTask;
        });
        for (var i = 0; i < 10; i++)
        {
            await asyncObservable.BroadcastAsync(2333);
        }

        Assert.AreEqual(10, totalMessages);
        subscription.Unsubscribe();
        
        for (var i = 0; i < 20; i++)
        {
            await asyncObservable.BroadcastAsync(2333);
        }
        Assert.AreEqual(10, totalMessages);
    }

    [TestMethod]
    public void UnRegisterMultiTimesFailedTest()
    {
        var asyncObservable = new AsyncObservable<int>();
        var subscription = asyncObservable.Subscribe(_ => Task.CompletedTask);
        subscription.Unsubscribe();
        try
        {
            subscription.Unsubscribe();
            Assert.Fail();
        }
        catch (Exception)
        {
            // ignored
        }
    }
    
    [TestMethod]
    public async Task MultipleUnRegisterTest()
    {
        var totalMessages = 0;
        var totalMessages2 = 0;

        var asyncObservable = new AsyncObservable<int>(); 
        asyncObservable.Subscribe(_ =>
        {
            totalMessages++;
            return Task.CompletedTask;
        });
        var subscription2 = asyncObservable.Subscribe(_ =>
        {
            totalMessages2++;
            return Task.CompletedTask;
        });
        for (var i = 0; i < 10; i++)
        {
            await asyncObservable.BroadcastAsync(2333);
        }

        Assert.AreEqual(10, totalMessages);
        Assert.AreEqual(10, totalMessages2);

        subscription2.Unsubscribe();
        
        for (var i = 0; i < 20; i++)
        {
            await asyncObservable.BroadcastAsync(2333);
        }
        Assert.AreEqual(30, totalMessages);
        Assert.AreEqual(10, totalMessages2);
    }

    [TestMethod]
    public void GetListenerCountTest()
    {
        var asyncObservable = new AsyncObservable<int>();
        var sub1 = asyncObservable.Subscribe(_ => Task.CompletedTask);
        var sub2 = asyncObservable.Subscribe(_ => Task.CompletedTask);

        Assert.AreEqual(2, asyncObservable.GetListenerCount());
        sub2.Unsubscribe();
        Assert.AreEqual(1, asyncObservable.GetListenerCount());
        sub1.Unsubscribe();
        Assert.AreEqual(0, asyncObservable.GetListenerCount());
    }
    
    [TestMethod]
    public async Task TestFilteredObservable()
    {
        var subscribedMessages = 0;
        var asyncObservable = new AsyncObservable<int>();
        var filteredObservable = asyncObservable
            .Filter(i => i % 2 == 0);
        filteredObservable.Subscribe(_ =>
        {
            subscribedMessages++;
            return Task.CompletedTask;
        });
        var filteredCounter = filteredObservable.Counter();
        var sourceCounter = asyncObservable.Counter();
        for (var i = 0; i < 10; i++)
        {
            await asyncObservable.BroadcastAsync(i);
        }

        Assert.AreEqual(10, sourceCounter.Count);
        Assert.AreEqual(5, filteredCounter.Count);
        Assert.AreEqual(5, subscribedMessages);
    }
    
    [TestMethod]
    public async Task TestMappedObservable()
    {
        var asyncObservable = new AsyncObservable<int>();
        var saved = asyncObservable
            .Map(i => i * 2)
            .Stage();
        
        for (var i = 0; i < 10; i++)
        {
            await asyncObservable.BroadcastAsync(i);
        }

        Assert.AreEqual(18, saved.Stage);
    }
    
    [TestMethod]
    public async Task TestThrottledObservable()
    {
        var asyncObservable = new AsyncObservable<int>();
        var doSub = asyncObservable
            .Throttle(TimeSpan.FromMilliseconds(100))
            .Map(_ => DateTime.Now.Millisecond)
            .Do();
        
        var watch = new Stopwatch();
        watch.Start();
        for (var i = 0; i < 10; i++)
        {
            await asyncObservable.BroadcastAsync(i);
        }
        watch.Stop();
        Assert.IsTrue(watch.ElapsedMilliseconds >= 1000 - 10);
        
        doSub.Unsubscribe();
        watch.Restart();
        for (var i = 0; i < 10; i++)
        {
            await asyncObservable.BroadcastAsync(i);
        }
        watch.Stop();
        Assert.IsTrue(watch.ElapsedMilliseconds < 10);
    }
    
    [TestMethod]
    public async Task TestRepeatableObservable()
    {
        var asyncObservable = new AsyncObservable<int>();
        var counter = asyncObservable
            .Repeat(3)
            .Counter();
        
        for (var i = 0; i < 10; i++)
        {
            await asyncObservable.BroadcastAsync(i);
        }
        
        Assert.AreEqual(30, counter.Count);
    }
    
    [TestMethod]
    public async Task TestRepeatableObservable2()
    {
        var asyncObservable = new AsyncObservable<int>();
        var stage = asyncObservable
            .Repeat(3)
            .Stage();
        
        for (var i = 0; i < 10; i++)
        {
            await asyncObservable.BroadcastAsync(i);
            Assert.AreEqual(i, stage.Stage);
        }
    }
    
    [DataRow(10, 3, 8, 3)]
    [DataRow(10, 1, 9, 10)]
    [DataRow(10, 2, 9, 5)]
    [TestMethod]
    public async Task TestSampleObservable(int total, int every, int finalStaged, int consumedCount)
    {
        var asyncObservable = new AsyncObservable<int>();
        var counter = asyncObservable
            .Sample(every)
            .Counter();
        var stage = asyncObservable
            .Sample(every)
            .Stage();
        
        for (var i = 0; i < total; i++)
        {
            await asyncObservable.BroadcastAsync(i);
        }
        
        Assert.AreEqual(finalStaged, stage.Stage);
        Assert.AreEqual(consumedCount, counter.Count);
    }
    
    [TestMethod]
    public void InvalidSampleObservable()
    {
        var asyncObservable = new AsyncObservable<int>();
        try
        {
            asyncObservable.Sample(0);
            Assert.Fail();
        }
        catch (Exception)
        {
            // ignored
        }
    }
    
    [TestMethod]
    public async Task TestAggregateObservable()
    {
        var asyncObservable = new AsyncObservable<int>();
        var aggregated = asyncObservable
            .Aggregate(3)
            .Stage();
        
        for (var i = 0; i < 10; i++)
        {
            await asyncObservable.BroadcastAsync(i);
        }
        
        Assert.IsNotNull(aggregated.Stage);
        Assert.AreEqual(6,aggregated.Stage[0]);
        Assert.AreEqual(7,aggregated.Stage[1]);
        Assert.AreEqual(8,aggregated.Stage[2]);
    }
    
    [TestMethod]
    public void TestInvalidAggregateObservable()
    {
        var asyncObservable = new AsyncObservable<int>();
        try
        {
            asyncObservable.Aggregate(0);
            Assert.Fail();
        }
        catch (Exception)
        {
            // ignored
        }
    }

    [TestMethod]
    public async Task TestStageMessage()
    {
        var asyncObservable = new AsyncObservable<int>();
        var stage = asyncObservable.Stage();
        
        await asyncObservable.BroadcastAsync(2333);
        
        Assert.AreEqual(2333, stage.Stage);
    }
    
    [TestMethod]
    public async Task TestCounter()
    {
        var asyncObservable = new AsyncObservable<int>();
        var counter = asyncObservable.Counter();
        
        await asyncObservable.BroadcastAsync(2333);
        await asyncObservable.BroadcastAsync(2333);
        await asyncObservable.BroadcastAsync(2333);
        
        Assert.AreEqual(3, counter.Count);
    }
    
    [TestMethod]
    public async Task FullFeaturesTest()
    {
        var asyncObservable = new AsyncObservable<int>();
        var stage = asyncObservable
            .Filter(i => i % 2 == 0)// 0, 2, 4, 6, 8, 10, 12, 14, 16
            .Map(i => i * 100)// 0, 200, 400, 600, 800, 1000, 1200, 1400, 1600
            .Throttle(TimeSpan.FromMilliseconds(100))
            .Repeat(2) // 0, 0, [200], 200, 400, [400], 600, 600, [800], 800, 1000, [1000], 1200, 1200, [1400], 1400, 1600, [1600]
            .Sample(3)// 200, 400, 800, 1000, 1400, 1600
            .Aggregate(3)// [200, 400, 800], [1000, 1400, 1600]
            .Stage();
        
        for (var i = 0; i < 18; i++)
        {
            await asyncObservable.BroadcastAsync(i);
        }
        
        Assert.IsNotNull(stage.Stage);
        Assert.AreEqual(1000, stage.Stage[0]);
        Assert.AreEqual(1400, stage.Stage[1]);
        Assert.AreEqual(1600, stage.Stage[2]);
    }
}
