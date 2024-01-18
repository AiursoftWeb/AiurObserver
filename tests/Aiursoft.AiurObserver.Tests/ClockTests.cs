using Aiursoft.AiurObserver.Clock;
using Aiursoft.AiurObserver.DefaultConsumers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.AiurObserver.Tests;

[TestClass]
public class ClockTests
{
    [TestMethod]
    public async Task TestClock()
    {
        var clock = new ObservableClock(TimeSpan.FromSeconds(1));
        var counter = new MessageCounter<DateTime>();
        clock.Subscribe(counter);
        await Task.Factory.StartNew(() => clock.StartClock());
        await Task.Delay(900);
        Assert.AreEqual(0, counter.Count);
        await Task.Delay(200);
        Assert.AreEqual(1, counter.Count);
    }
}