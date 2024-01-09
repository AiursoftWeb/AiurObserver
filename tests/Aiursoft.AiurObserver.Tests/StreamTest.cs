using Aiursoft.AiurObserver.Stream;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        var stream = File.OpenRead(testFile);
        
        // Create an observable stream.
        var observableStream = new ObservableStream(stream);

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