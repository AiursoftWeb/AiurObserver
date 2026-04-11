# 📜 IO Streams with AiurObserver

The `Aiursoft.AiurObserver.Stream` package bridges the gap between traditional `System.IO.Stream` (pull-based, byte-oriented) and AiurObserver (push-based, object-oriented). It turns any stream into an observable that emits strings line-by-line.

---

## 🧭 Scenarios

- [Real-time Log Processor](#-real-time-log-processor)
- [Batch Data Uploader](#-batch-data-uploader)
- [The Stream-to-WebSocket Bridge](#-the-stream-to-websocket-bridge)
- [Slow Consumer Protection](#-slow-consumer-protection)

---

## 🔍 Real-time Log Processor

Turn a file stream into a searchable, formatted log monitor using `Filter` and `Map`.

```csharp
using var fileStream = File.OpenRead("app.log");
var observable = fileStream.ToObservableStream();

// Define the processing pipeline
observable
    .Filter(line => line.Contains("ERROR") || line.Contains("FATAL"))
    .Map(line => $"[ALERT] {DateTime.Now}: {line}")
    .Subscribe(formattedLine => 
    {
        Console.WriteLine(formattedLine);
        return Task.CompletedTask;
    });

// Start reading
await observable.ReadToEndAsync();
```

---

## 📦 Batch Data Uploader

When reading a large file (like a CSV), you often don't want to process lines one-by-one. Use `Aggregate` to group lines into batches for efficient database insertion.

```csharp
using var csvStream = File.OpenRead("data.csv");

csvStream.ToObservableStream()
    .Filter(line => !string.IsNullOrWhiteSpace(line)) // Skip empty lines
    .Aggregate(100) // Group into batches of 100 lines
    .Subscribe(async batch => 
    {
        // batch is a string[]
        await _myDatabase.BulkInsertAsync(batch);
        Console.WriteLine($"Uploaded {batch.Length} items.");
    });

await csvStream.ToObservableStream().ReadToEndAsync();
```

---

## 🌉 The Stream-to-WebSocket Bridge

This pattern allows you to "pipe" the contents of a file or a network stream directly to a remote client.

```csharp
var ws = await context.AcceptWebSocketClient();
using var logFile = File.OpenRead("live-server.log");

logFile.ToObservableStream()
    .Map(line => $"[REMOTE-LOG] {line}")
    .Subscribe(ws); // Send every line to the WebSocket

await logFile.ToObservableStream().ReadToEndAsync();
```

---

## 🛡️ Slow Consumer Protection

If your source stream is fast (like a local file) but your consumer is slow (like a remote API call), the `WithBuffer` operator ensures the reading process isn't bottlenecked while preventing memory spikes.

```csharp
using var largeFile = File.OpenRead("huge-dump.txt");

largeFile.ToObservableStream()
    .WithBuffer(500) // Buffer up to 500 lines if the consumer is slow
    .MapAsync(async line => 
    {
        await _slowApi.PostAsync(line); // Slow network operation
        return line;
    })
    .Subscribe();

await largeFile.ToObservableStream().ReadToEndAsync();
```

---

## 🛠️ Combined: Clock + Stream (Log Rotation Check)

**Example: Check a log file every 10 seconds to see if it has been updated.**

```csharp
var clock = new ObservableClock(TimeSpan.FromSeconds(10));
var lastCount = 0;

clock.Subscribe(async _ => 
{
    using var fs = File.OpenRead("activity.log");
    var stream = fs.ToObservableStream();
    var counter = stream.Counter();
    
    await stream.ReadToEndAsync();
    
    if (counter.Count > lastCount)
    {
        Console.WriteLine($"Log grew! Added {counter.Count - lastCount} lines.");
        lastCount = counter.Count;
    }
});

await clock.StartClock();
```
