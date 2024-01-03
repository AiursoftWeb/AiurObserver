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
        var totalMessages = 0;
        var asyncObservable = new AsyncObservable<int>();
        asyncObservable
            .Filter(i => i % 2 == 0)
            .Subscribe(_ =>
            {
                totalMessages++;
                return Task.CompletedTask;
            });
        for (var i = 0; i < 10; i++)
        {
            await asyncObservable.BroadcastAsync(i);
        }

        Assert.AreEqual(5, totalMessages);
    }
    
    [TestMethod]
    public async Task TestMappedObservable()
    {
        var totalMessages = 0;
        var asyncObservable = new AsyncObservable<int>();
        var saved = asyncObservable
            .Map(i => i * 2)
            .Keep();
       
        for (var i = 0; i < 10; i++)
        {
            await asyncObservable.BroadcastAsync(i);
        }

        Assert.AreEqual(10, totalMessages);
        Assert.AreEqual(18, saved.Last);
    }

    [TestMethod]
    public async Task TestKeepMessage()
    {
        var asyncObservable = new AsyncObservable<int>();
        var saved = asyncObservable.Keep();
        
        await asyncObservable.BroadcastAsync(2333);
        
        Assert.AreEqual(2333, saved.Last);
    }
}
