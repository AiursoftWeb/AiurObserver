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
    public async Task RadioTest()
    {
        var asyncObservable = new AsyncObservable<int>();
        var radio = new MessageRadio<int>();
        asyncObservable.Subscribe(radio);
        
        var stage1 = new MessageStageLast<int>();
        var stage2 = new MessageStageLast<int>();
        var stage3 = new MessageStageLast<int>();
        radio.Subscribe(stage1);
        radio.Subscribe(stage2);
        radio.Subscribe(stage3);
        
        await asyncObservable.BroadcastAsync(2333);
        
        Assert.AreEqual(2333, stage1.Stage);
        Assert.AreEqual(2333, stage2.Stage);
        Assert.AreEqual(2333, stage3.Stage);
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
        var filteredCounter = new MessageCounter<int>();
        filteredObservable.Subscribe(filteredCounter);
        var sourceCounter = new MessageCounter<int>();
        asyncObservable.Subscribe(sourceCounter);
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
        var saved = new MessageStageLast<int>();
        asyncObservable
            .Map(i => i * 2)
            .Subscribe(saved);

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
            .Subscribe();

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
        var counter = new MessageCounter<int>(); 
        asyncObservable
            .Repeat(3)
            .Subscribe(counter);

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
        
        var stage = new MessageStageLast<int>(); 
        asyncObservable
            .Repeat(3)
            .Subscribe(stage);

        for (var i = 0; i < 10; i++)
        {
            await asyncObservable.BroadcastAsync(i);
            Assert.AreEqual(i, stage.Stage);
        }
    }

    [TestMethod]
    public async Task TestWhenMessagesObservable()
    {
        var asyncObservable = new AsyncObservable<int>();
        var counter = new MessageCounter<int>();
        var stage = new MessageStageFirst<int>();
        asyncObservable.Subscribe(counter);
        counter.Filter(c => c == 2).Subscribe(stage);

        await asyncObservable.BroadcastAsync(23333);
        Assert.IsFalse(stage.IsStaged);
        Assert.AreEqual(1, counter.Count);
        await asyncObservable.BroadcastAsync(23333);
        await asyncObservable.BroadcastAsync(233333);
        Assert.IsTrue(stage.IsStaged);
        Assert.AreEqual(3, counter.Count);
    }

    [DataRow(10, 3, 8, 3)]
    [DataRow(10, 1, 9, 10)]
    [DataRow(10, 2, 9, 5)]
    [TestMethod]
    public async Task TestSampleObservable(int total, int every, int finalStaged, int consumedCount)
    {
        var asyncObservable = new AsyncObservable<int>();
        var counter = new MessageCounter<int>();
        asyncObservable
            .Sample(every)
            .Subscribe(counter);
        var stageLast = new MessageStageLast<int>();
        asyncObservable
            .Sample(every)
            .Subscribe(stageLast);

        for (var i = 0; i < total; i++)
        {
            await asyncObservable.BroadcastAsync(i);
        }

        Assert.AreEqual(finalStaged, stageLast.Stage);
        Assert.AreEqual(consumedCount, counter.Count);
    }
    
    [TestMethod]
    public async Task TestSampleDoObservable()
    {
        var myCounter = 0;
        var asyncObservable = new AsyncObservable<int>();
        var counter = new MessageCounter<int>();
        asyncObservable
            .SampleDo(3, _ =>
            {
                myCounter++;
                return Task.CompletedTask;
            })
            .Subscribe(counter);

        for (var i = 0; i < 10; i++)
        {
            await asyncObservable.BroadcastAsync(i);
        }

        Assert.AreEqual(10, counter.Count);
        Assert.AreEqual(3, myCounter);
    }
    
    [TestMethod]
    public void TestInvalidSampleDoObservable()
    {
        var asyncObservable = new AsyncObservable<int>();
        try
        {
            asyncObservable.SampleDo(0, _ => Task.CompletedTask);
            Assert.Fail();
        }
        catch (Exception)
        {
            // ignored
        }
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
        var aggregated = new MessageStageLast<int[]>();
        asyncObservable
            .Aggregate(3)
            .Subscribe(aggregated);

        for (var i = 0; i < 10; i++)
        {
            await asyncObservable.BroadcastAsync(i);
        }

        Assert.IsNotNull(aggregated.Stage);
        Assert.AreEqual(6, aggregated.Stage[0]);
        Assert.AreEqual(7, aggregated.Stage[1]);
        Assert.AreEqual(8, aggregated.Stage[2]);
    }
    
    [TestMethod]
    public async Task TestSplitObservable()
    {
        var asyncObservable = new AsyncObservable<int[]>();
        var counter = new MessageCounter<int>();
        asyncObservable
            .ForEach()
            .Subscribe(counter);

        await asyncObservable.BroadcastAsync(new[] { 1, 2, 3 });
        await asyncObservable.BroadcastAsync(new[] { 1, 2, 3 });
        Assert.AreEqual(6, counter.Count);
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
    public async Task TestAggregateThenSplitObservable()
    {
        var asyncObservable = new AsyncObservable<int>();
        var counter = new MessageCounter<int>();
        asyncObservable
            .Aggregate(3)
            .ForEach()
            .Subscribe(counter);

        await asyncObservable.BroadcastAsync(1);
        await asyncObservable.BroadcastAsync(2);
        await asyncObservable.BroadcastAsync(3);
        await asyncObservable.BroadcastAsync(4);
        await asyncObservable.BroadcastAsync(5);
        await asyncObservable.BroadcastAsync(6);
        Assert.AreEqual(6, counter.Count);
    }

    [TestMethod]
    public async Task TestInNewThread()
    {
        var asyncObservable = new AsyncObservable<int>();
        var watch = new Stopwatch();
        watch.Start();

        var stage = new MessageStageLast<int>();
        asyncObservable
            .InNewThread()
            .Throttle(TimeSpan.FromMilliseconds(100))
            .Subscribe(stage);

        for (var i = 0; i < 10; i++)
        {
            await asyncObservable.BroadcastAsync(i);
        }

        Assert.IsTrue(watch.ElapsedMilliseconds < 50, watch.ElapsedMilliseconds.ToString());

        // Count to 4
        while (stage.Stage != 4)
        {
            await Task.Delay(10);
        }

        Assert.IsTrue(watch.ElapsedMilliseconds >= 400, watch.ElapsedMilliseconds.ToString());
        Assert.IsTrue(watch.ElapsedMilliseconds <= 600, watch.ElapsedMilliseconds.ToString());

        // Count to 9
        while (stage.Stage != 9)
        {
            await Task.Delay(10);
        }

        watch.Stop();
        Assert.IsTrue(watch.ElapsedMilliseconds >= 990, watch.ElapsedMilliseconds.ToString());
        Assert.IsTrue(watch.ElapsedMilliseconds <= 1200, watch.ElapsedMilliseconds.ToString());
    }

    [TestMethod]
    public async Task TestDelayedInNewThread()
    {
        var asyncObservable = new AsyncObservable<int>();
        var watch = new Stopwatch();
        watch.Start();

        var stage = new MessageStageFirst<int>();
        asyncObservable
            .InNewThread()
            .Delay(TimeSpan.FromMilliseconds(200))
            .Subscribe(stage);

        for (var i = 0; i < 10; i++)
        {
            await asyncObservable.BroadcastAsync(i);
        }

        Assert.IsTrue(watch.ElapsedMilliseconds < 50, watch.ElapsedMilliseconds.ToString());
        Assert.AreEqual(false, stage.IsStaged);
        while (!stage.IsStaged)
        {
            await Task.Delay(10);
        }

        watch.Stop();
        Assert.IsTrue(watch.ElapsedMilliseconds >= 200, watch.ElapsedMilliseconds.ToString());
        Assert.IsTrue(watch.ElapsedMilliseconds <= 300, watch.ElapsedMilliseconds.ToString());
    }

    [TestMethod]
    public async Task TestLocked()
    {
        var asyncObservable = new AsyncObservable<int>();
        var watch = new Stopwatch();
        watch.Start();
        
        var counter = new MessageCounter<int>();
        asyncObservable
            .InNewThread()
            .Delay(TimeSpan.FromMilliseconds(100))
            .LockOneThread()
            .Subscribe(counter);

        for (var i = 0; i < 10; i++)
        {
            await asyncObservable.BroadcastAsync(i);
        }

        Assert.IsTrue(watch.ElapsedMilliseconds < 50, watch.ElapsedMilliseconds.ToString());

        while (counter.Count != 10)
        {
            await Task.Delay(3);
        }

        watch.Stop();
        Assert.IsTrue(watch.ElapsedMilliseconds >= 100, watch.ElapsedMilliseconds.ToString());
        Assert.IsTrue(watch.ElapsedMilliseconds <= 140, watch.ElapsedMilliseconds.ToString());
    }

    [TestMethod]
    public async Task TestStageLastMessage()
    {
        var asyncObservable = new AsyncObservable<int>();
        var stage = new MessageStageLast<int>();
        asyncObservable.Subscribe(stage);
        Assert.IsFalse(stage.IsStaged);

        await asyncObservable.BroadcastAsync(33344);
        await asyncObservable.BroadcastAsync(44455);
        await asyncObservable.BroadcastAsync(2333);

        Assert.IsTrue(stage.IsStaged);
        Assert.AreEqual(2333, stage.Stage);
    }
    
    [TestMethod]
    public async Task TestStageSpecificMessage()
    {
        var asyncObservable = new AsyncObservable<int>();
        var stage = new MessageStageSpecific<int>(1);
        asyncObservable.Subscribe(stage);
        Assert.IsFalse(stage.IsStaged);

        await asyncObservable.BroadcastAsync(33344);
        await asyncObservable.BroadcastAsync(44455);
        await asyncObservable.BroadcastAsync(2333);

        Assert.IsTrue(stage.IsStaged);
        Assert.AreEqual(44455, stage.Stage);
    }
    
    [TestMethod]
    public async Task TestAdder()
    {
        var asyncObservable = new AsyncObservable<double>();
        var adder = new MessageAdder<double>();
        asyncObservable.Subscribe(adder);

        await asyncObservable.BroadcastAsync(2333);
        await asyncObservable.BroadcastAsync(33344);
        await asyncObservable.BroadcastAsync(44455);

        Assert.AreEqual(80132, adder.Sum);
    }
    [TestMethod]
    public async Task TestAverageRecent()
    {
        var asyncObservable = new AsyncObservable<int>();
        
        var averageRecent = new RecentMessageAverage<int>(3);
        asyncObservable.Subscribe(averageRecent);
        
        var averageTotal = new MessageAverage<int>();
        asyncObservable.Subscribe(averageTotal);
        
        await asyncObservable.BroadcastAsync(1);

        var (t1, c1) = averageRecent.Average(); 
        var (t2, c2) = averageTotal.Average();
        Assert.AreEqual(1, t1 / c1);
        Assert.AreEqual(1, t2 / c2);

        await asyncObservable.BroadcastAsync(2);
        await asyncObservable.BroadcastAsync(3);
        
        (t1, c1) = averageRecent.Average(); 
        (t2, c2) = averageTotal.Average();
        Assert.AreEqual(2, t1 / c1);
        Assert.AreEqual(2, t2 / c2);

        await asyncObservable.BroadcastAsync(3);
        await asyncObservable.BroadcastAsync(3);

        (t1, c1) = averageRecent.Average(); 
        (t2, c2) = averageTotal.Average();
        Assert.AreEqual(3, t1 / c1);
        Assert.AreEqual(2.4, (double)t2 / c2);
    }

    [TestMethod]
    public async Task TestFirstMessage()
    {
        var asyncObservable = new AsyncObservable<int>();
        var first = new MessageStageFirst<int>();
        asyncObservable.Subscribe(first);
        Assert.IsFalse(first.IsStaged);

        await asyncObservable.BroadcastAsync(2333);
        await asyncObservable.BroadcastAsync(33344);
        await asyncObservable.BroadcastAsync(44455);

        Assert.AreEqual(2333, first.Stage);
        Assert.IsTrue(first.IsStaged);
    }

    [TestMethod]
    public async Task TestCounter()
    {
        var asyncObservable = new AsyncObservable<int>();
        var counter = new MessageCounter<int>();
        asyncObservable.Subscribe(counter);

        await asyncObservable.BroadcastAsync(2333);
        await asyncObservable.BroadcastAsync(2333);
        await asyncObservable.BroadcastAsync(2333);

        Assert.AreEqual(3, counter.Count);
    }

    [TestMethod]
    public async Task FullFeaturesTest()
    {
        int events = 0;
        var asyncObservable = new AsyncObservable<int>();
        var stage = new MessageStageLast<int[]>();
        asyncObservable
            .Throttle(TimeSpan.FromMilliseconds(100))
            .Filter(i => i % 2 == 0) // 0, 2, 4, 6, 8, 10, 12, 14, 16
            .Map(i => i * 100) // 0, 200, 400, 600, 800, 1000, 1200, 1400, 1600
            .Repeat(2) // 0, 0, [200], 200, 400, [400], 600, 600, [800], 800, 1000, [1000], 1200, 1200, [1400], 1400, 1600, [1600]
            .Sample(3) // 200, 400, 800, 1000, 1400, 1600
            .Pipe(_ => events++)
            .Aggregate(3) // [200, 400, 800], [1000, 1400, 1600]
            .Subscribe(stage);

        for (var i = 0; i < 18; i++)
        {
            await asyncObservable.BroadcastAsync(i);
        }

        Assert.AreEqual(6, events);
        Assert.IsNotNull(stage.Stage);
        Assert.AreEqual(1000, stage.Stage[0]);
        Assert.AreEqual(1400, stage.Stage[1]);
        Assert.AreEqual(1600, stage.Stage[2]);
    }
}