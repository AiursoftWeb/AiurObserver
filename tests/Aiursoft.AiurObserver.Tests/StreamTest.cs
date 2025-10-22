using Aiursoft.AiurObserver.DefaultConsumers;
using Aiursoft.AiurObserver.Stream;

namespace Aiursoft.AiurObserver.Tests;

[TestClass]
public class StreamTest
{
    [TestMethod]
    public async Task TestStream()
    {
        var testFile = Path.GetTempFileName();
        
        // Insert 10 lines into the file.
        await File.WriteAllLinesAsync(testFile, Enumerable.Range(0, 10).Select(t => t.ToString()));
        
        // Open the file for reading.
        var observableStream = File.OpenRead(testFile).ToObservableStream();
        
        // Create a counter to count the number of lines.
        var counter = new MessageCounter<string>();
        var stage = new MessageStageLast<string>();
        observableStream.Subscribe(counter);
        observableStream.Subscribe(stage);
        await observableStream.ReadToEndAsync();
        
        Assert.AreEqual("9", stage.Stage);
        Assert.AreEqual(10, counter.Count);
    }
}