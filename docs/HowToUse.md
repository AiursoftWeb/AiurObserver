# AiurObserver

AiurObserver is a lightweight, asynchronous C\# implementation of the Observer design pattern (also known as Reactive programming). It's inspired by Rx.NET but simplified for modern `async/await` workflows. It allows you to construct objects that can be observed asynchronously and provides a set of operators to manipulate and process data streams.

## 🧭 Table of Contents

* [Installation](#installation)
* [Core Concepts](#core-concepts)
* [The AiurObserver Pattern (vs. IEnumerable)](#-the-aiurobserver-pattern-vs-ienumerable)
* [Fundamental Rules](#-fundamental-rules)
* [Built-in Consumers](#-built-in-consumers)
* [Chaining Operators (Features)](#-chaining-operators-features)
* [Concurrency Operators](#-concurrency-operators)
* [Utility Operators](#-utility-operators)
* [Error Handling](#-error-handling)
* [Full Example](#-full-example)

-----

## Installation

You can install AiurObserver via the NuGet Package Manager console:

```powershell
Install-Package Aiursoft.AiurObserver
```

-----

## Core Concepts

The entire library is built on three simple interfaces:

1.  **`IAsyncObservable<T>` (The Source):** This is the "subject" or "source." It's the object that will `BroadcastAsync` new values. You `Subscribe` to it.
2.  **`IConsumer<T>` (The Listener):** This is the "observer" or "listener." It's an object with a `Consume(T value)` method that reacts to new values.
3.  **`ISubscription` (The Link):** This is the object returned by `Subscribe()`. It represents the connection between the observable and the consumer. Call `Unsubscribe()` on it to sever the connection.

-----

## 🔀 The AiurObserver Pattern (vs. IEnumerable)

`IEnumerable` follows a **"pull"** pattern:

* Data source
* Query
* Do Next (The consumer *pulls* data when it's ready)

<!-- end list -->

```csharp
// 1. Data source
var list = new List<int> { 1, 2, 3, 4, 5 };

// 2. Query
var query = list.Where(t => t >= 1);

var data = query.ToList();
// 3. Do Next: The `foreach` loop pulls data from the query.
foreach (var item in data)
{
    // ...
}
```

AiurObserver follows a **"push"** pattern:

* Query
* Do Next
* Data source (The source *pushes* data to the consumer)

<!-- end list -->

```csharp
// 1. Query
var asyncObservable = new AsyncObservable<int>();
var query = asyncObservable.Filter(t => t >= 1);

// 2. Do Next: Define *what to do* when data arrives.
var subscription = query.Subscribe(t => 
{
    // This code runs whenever the source pushes data.
});

// 3. Data source: Push data at any time.
await asyncObservable.BroadcastAsync(1);
await asyncObservable.BroadcastAsync(2);
await asyncObservable.BroadcastAsync(3);
```

It works asynchronously. You can broadcast data to the observable at any time, and the consumer you defined will be triggered at any time.

-----

## 📜 Fundamental Rules

There are three core rules to using AiurObserver successfully.

### 1\. Basic Usage

You `Subscribe` to an `AsyncObservable` to get an `ISubscription`. You use `BroadcastAsync` to send values.

```csharp
var totalMessages = 0;
var asyncObservable = new AsyncObservable<int>();
var subscription = asyncObservable.Subscribe(_ =>
{
    totalMessages++;
    return Task.CompletedTask;
});
await asyncObservable.BroadcastAsync(2333);
Console.WriteLine(totalMessages); // 1
```

### 2\. Always Consume

You **must** consume the observable. A chain of operators (`Filter`, `Map`, etc.) does nothing by itself. It's just a *definition* of a query. Nothing happens until a consumer is attached using `Subscribe()` or a built-in consumer (like `Counter()`).

```csharp
var asyncObservable = new AsyncObservable<int>();
var query = asyncObservable.Filter(t => t > 100); // query is another IAsyncObservable, but no one is listening.

// Nothing happens!
await asyncObservable.BroadcastAsync(2333); 

var subscription = query.Subscribe(t => Console.WriteLine(t)); // Now we are listening.
await asyncObservable.BroadcastAsync(2333); // 2333
```

### 3\. Always Unsubscribe

If the observable is no longer needed, you **must** unsubscribe to prevent memory leaks and unwanted behavior. The `ISubscription` object implements `IDisposable`, so the easiest way to manage this is with a `using` block or a `try/finally`.

```csharp
ISubscription? subscription = null;
try
{
    subscription = _someService.Subscribe(t => Console.WriteLine(t));

    // Don't forget the `await` here!
    await asyncObservable.BroadcastAsync(2333);
}
finally
{
    // This detaches the consumer from the observable.
    subscription?.Unsubscribe();
}

// Or, with using (if the subscription's lifetime is known):
using var sub = asyncObservable.Subscribe(t => Console.WriteLine(t));
await asyncObservable.BroadcastAsync(1);
// sub.Unsubscribe() is called automatically here.
```

-----

## 📦 Built-in Consumers

AiurObserver provides basic consumers for common tasks. These are "terminal" operators—they must be at the **end** of an observable chain, and you don't call `Subscribe` on them.

```csharp
var asyncObservable = new AsyncObservable<int>();
var counter = asyncObservable
    .Counter(); // This is the consumer.

// Then Access:
var count = counter.Count;
```

### Counter

`Counter()` will count how many times the observable has been broadcasted.

```csharp
var asyncObservable = new AsyncObservable<int>();
var counter = asyncObservable.Counter();

await asyncObservable.BroadcastAsync(2333);
await asyncObservable.BroadcastAsync(2333);
await asyncObservable.BroadcastAsync(2333);

Console.WriteLine(counter.Count); // 3
```

### Stage First

`StageFirst()` will keep only the **first** broadcasted message it receives.

```csharp
var asyncObservable = new AsyncObservable<int>();
var first = asyncObservable.StageFirst();

Console.WriteLine(first.IsStaged); // False

await asyncObservable.BroadcastAsync(2333);
await asyncObservable.BroadcastAsync(33344);
await asyncObservable.BroadcastAsync(44455);

Console.WriteLine(first.Stage); // 2333
Console.WriteLine(first.IsStaged); // True
```

### Stage Last

`StageLast()` will keep only the **last** broadcasted message it receives.

```csharp
var asyncObservable = new AsyncObservable<int>();
var stage = asyncObservable.StageLast();
Console.WriteLine(stage.IsStaged); // False

await asyncObservable.BroadcastAsync(2333);
await asyncObservable.BroadcastAsync(33344);
await asyncObservable.BroadcastAsync(44455);

Console.WriteLine(stage.Stage); // 44455
Console.WriteLine(stage.IsStaged); // True
```

### Waiting for Events with StageFirst/StageLast

Both `StageFirst` and `StageLast` provide a `WaitOneEvent()` helper method. This returns a `Task<T>` that completes when the *next* event is staged, allowing you to `await` a broadcast in your procedural code.

```csharp
var asyncObservable = new AsyncObservable<int>();
var messageStage = asyncObservable.StageLast();

// Start waiting for the next event
var waitTask = messageStage.WaitOneEvent();

// Ensure the task is not completed yet
Assert.IsFalse(waitTask.IsCompleted);

// Broadcast an event
await asyncObservable.BroadcastAsync(42);

// Wait for the event to be received
var result = await waitTask;

// Ensure the result is as expected
Assert.AreEqual(42, result);
Assert.IsTrue(messageStage.IsStaged);
Assert.AreEqual(42, messageStage.Stage);
```

### Adder

`Adder()` will sum all broadcasted messages. It only works when the message type is numeric.

```csharp
var asyncObservable = new AsyncObservable<double>();
var summer = asyncObservable.Adder();

await asyncObservable.BroadcastAsync(2333);
await asyncObservable.BroadcastAsync(33344);
await asyncObservable.BroadcastAsync(44455);

Console.WriteLine(summer.Sum); // 80132
```

### Average

`Average()` will calculate the average of all broadcasted messages. It only works when the message type is numeric. It returns the `Total` and `Count` so you can calculate the average.

```csharp
var asyncObservable = new AsyncObservable<int>();
var average = asyncObservable.Average();

await asyncObservable.BroadcastAsync(1);

var (totalA, countA) = average.Average(); // 1 / 1
Console.WriteLine((double)totalA / countA); // 1.0

await asyncObservable.BroadcastAsync(2);
await asyncObservable.BroadcastAsync(3);
await asyncObservable.BroadcastAsync(3);
await asyncObservable.BroadcastAsync(3);

(totalA, countA) = average.Average(); // 12 / 5
Console.WriteLine((double)totalA / countA); // 2.4
```

### Average Recent

`AverageRecent(int n)` will calculate the average of the last **N** broadcasted messages. It only works when the message type is numeric.

```csharp
var asyncObservable = new AsyncObservable<int>();
var averageRecent = asyncObservable.AverageRecent(3); // Keep a buffer of 3

await asyncObservable.BroadcastAsync(1);

var (total, count) = averageRecent.Average(); // 1 / 1
Console.WriteLine((double)total / count); // 1.0

await asyncObservable.BroadcastAsync(3);
await asyncObservable.BroadcastAsync(3);
await asyncObservable.BroadcastAsync(3);

(total, count) = averageRecent.Average(); // (3 + 3 + 3) / 3
Console.WriteLine((double)total / count); // 3.0
```

-----

## ⛓️ Chaining Operators (Features)

The real power of AiurObserver comes from its LINQ-like chaining operators. You can combine these to build complex, asynchronous data processing pipelines. Each operator returns a new `IAsyncObservable`, so you can chain them together fluently.

### Filter

`Filter(t => ...)` will only allow messages to pass through to the next part of the chain if they satisfy the given predicate (a function that returns `true` or `false`).

```csharp
var asyncObservable = new AsyncObservable<int>();
var subscription = asyncObservable
    .Filter(t => t % 2 == 0) // Only allow even numbers
    .Subscribe(t => Console.WriteLine(t));

await asyncObservable.BroadcastAsync(1); // Blocked
await asyncObservable.BroadcastAsync(2); // 2
await asyncObservable.BroadcastAsync(3); // Blocked
await asyncObservable.BroadcastAsync(4); // 4
```

-----

### Map

`Map(t => ...)` transforms each message into a new form. This can also change the type of the observable stream (e.g., from `IAsyncObservable<int>` to `IAsyncObservable<string>`).

```csharp
var asyncObservable = new AsyncObservable<int>();
var subscription = asyncObservable
    .Map(t => $"Number: {t}") // Transform int to string
    .Subscribe(t => Console.WriteLine(t));

await asyncObservable.BroadcastAsync(1); // "Number: 1"
await asyncObservable.BroadcastAsync(2); // "Number: 2"
```

-----

### MapAsync

`MapAsync(async t => ...)` is an asynchronous version of `Map`. It's used when your transformation logic involves an `await`able operation (like a network request, database query, or a simple `Task.Delay`).

```csharp
var asyncObservable = new AsyncObservable<int>();
var subscription = asyncObservable
    .MapAsync(async t => 
    {
        await Task.Delay(100); // Simulate async work
        return $"Async processed: {t}";
    })
    .Subscribe(t => Console.WriteLine(t));

await asyncObservable.BroadcastAsync(1); // Will print "Async processed: 1" after 100ms
```

-----

### Pipe

`Pipe(t => ...)` allows you to perform a "side effect" action on a message as it passes through the chain, without modifying the message itself. It's perfect for logging or debugging your chain.

```csharp
var asyncObservable = new AsyncObservable<int>();
var subscription = asyncObservable
    .Filter(t => t > 1)
    .Pipe(t => Console.WriteLine($"[Pipe] Saw: {t}")) // Side effect
    .Map(t => t * 10)
    .Subscribe(t => Console.WriteLine($"[Subscribe] Got: {t}"));

await asyncObservable.BroadcastAsync(2);
// Output:
// [Pipe] Saw: 2
// [Subscribe] Got: 20
```

-----

### Throttle

`Throttle(TimeSpan)` enforces a minimum time gap between messages. It ensures that a consumer is not overwhelmed by rapid-fire broadcasts. It effectively rate-limits the stream, ensuring that the processing of one message completes and the time gap passes before the next one begins.

```csharp
var asyncObservable = new AsyncObservable<int>();
var subscription = asyncObservable
    .Throttle(TimeSpan.FromMilliseconds(100))
    .Subscribe(t => Console.WriteLine($"{DateTime.Now.TimeOfDay}: {t}"));

// These will all be broadcast instantly...
await asyncObservable.BroadcastAsync(1);
await asyncObservable.BroadcastAsync(2);
await asyncObservable.BroadcastAsync(3);

// ...but the output will be spaced out by 100ms each.
// 12:00:01.100: 1
// 12:00:01.200: 2
// 12:00:01.300: 3
```

-----

### Repeat

`Repeat(int times)` will duplicate every message that reaches it, broadcasting it downstream N times before processing the next message.

```csharp
var asyncObservable = new AsyncObservable<int>();
var subscription = asyncObservable
    .Repeat(3)
    .Subscribe(t => Console.WriteLine(t));

await asyncObservable.BroadcastAsync(1);
// Output:
// 1
// 1
// 1
```

-----

### Sample

`Sample(int every)` will only let every Nth message pass through. All other messages are dropped.

```csharp
var asyncObservable = new AsyncObservable<int>();
var subscription = asyncObservable
    .Sample(3) // Only let the 3rd, 6th, 9th... message pass
    .Subscribe(t => Console.WriteLine(t));

await asyncObservable.BroadcastAsync(1); // Blocked
await asyncObservable.BroadcastAsync(2); // Blocked
await asyncObservable.BroadcastAsync(3); // 3
await asyncObservable.BroadcastAsync(4); // Blocked
await asyncObservable.BroadcastAsync(5); // Blocked
await asyncObservable.BroadcastAsync(6); // 6
```

-----

### SampleDo

`SampleDo(int every, Func<T, Task> action)` is a specialized operator. Unlike `Sample`, it **lets all messages pass through**. However, it also performs an asynchronous `action` on every Nth message.

```csharp
var asyncObservable = new AsyncObservable<int>();
var subscription = asyncObservable
    .SampleDo(2, async t => 
    {
        // This action runs on message 2 and 4
        Console.WriteLine($"[SampleDo Action] Logging: {t}");
    }) 
    .Subscribe(t => Console.WriteLine($"[Subscribe] Got: {t}"));

await asyncObservable.BroadcastAsync(1); // [Subscribe] Got: 1
await asyncObservable.BroadcastAsync(2); // [SampleDo Action] Logging: 2 -> [Subscribe] Got: 2
await asyncObservable.BroadcastAsync(3); // [Subscribe] Got: 3
await asyncObservable.BroadcastAsync(4); // [SampleDo Action] Logging: 4 -> [Subscribe] Got: 4
```

-----

### Aggregate

`Aggregate(int every)` collects messages into a buffer until it has N items, and then broadcasts that buffer as a single array (`T[]`). It's also known as `Buffer` or `Batch` in other libraries.

```csharp
var asyncObservable = new AsyncObservable<int>();
var subscription = asyncObservable
    .Aggregate(3) // Group messages in batches of 3
    .Subscribe(batch => 
    {
        // batch is an int[]
        Console.WriteLine($"Got batch: {string.Join(", ", batch)}");
    });

await asyncObservable.BroadcastAsync(1);
await asyncObservable.BroadcastAsync(2);
await asyncObservable.BroadcastAsync(3); // "Got batch: 1, 2, 3"
await asyncObservable.BroadcastAsync(4);
await asyncObservable.BroadcastAsync(5);
await asyncObservable.BroadcastAsync(6); // "Got batch: 4, 5, 6"
```

-----

### ForEach

`ForEach()` is the opposite of `Aggregate`. It takes a stream where each message is an array (`T[]`) and splits it, broadcasting each item in the array as an individual message.

```csharp
var asyncObservable = new AsyncObservable<int[]>();
var subscription = asyncObservable
    .ForEach()
    .Subscribe(t => Console.WriteLine(t)); // t is an int

await asyncObservable.BroadcastAsync(new[] { 1, 2, 3 });
// Output:
// 1
// 2
// 3
```

-----

## ⚡ Concurrency Operators

These operators give you fine-grained control over threading and asynchrony.

### InNewThread

`InNewThread(Action<Exception>? onError = null)` moves all subsequent operations (like `Map`, `Filter`, and the final `Subscribe`) onto a background thread (`Task.Factory.StartNew`). This is crucial for unblocking the broadcaster, allowing `BroadcastAsync` to return *immediately* even if the consumer is slow.

If an exception happens in the consumer thread, it is caught and passed to the `onError` handler (defaulting to `Console.Error.WriteLine`) instead of crashing the process.

```csharp
var asyncObservable = new AsyncObservable<int>();
var subscription = asyncObservable
    .InNewThread()
    .MapAsync(async t => 
    {
        await Task.Delay(1000); // 1-second delay
        return t;
    })
    .Subscribe(t => Console.WriteLine(t));

Console.WriteLine("Broadcasting 1");
await asyncObservable.BroadcastAsync(1); // This call returns *instantly*
Console.WriteLine("Broadcasting 2");
await asyncObservable.BroadcastAsync(2); // This call returns *instantly*
Console.WriteLine("Done broadcasting");

// 1 second later: 1
// 1 second later: 2
```

-----

### LockOneThread

`LockOneThread()` is a concurrency utility. It uses a `SemaphoreSlim` to ensure that even if the stream is processed on multiple threads (e.g., via `InNewThread`), only one message is processed by the rest of the pipeline at a time. It serializes access.

```csharp
var asyncObservable = new AsyncObservable<int>();
var subscription = asyncObservable
    .InNewThread()     // Process on background threads
    .LockOneThread()   // But only one at a time
    .MapAsync(async t => 
    {
        Console.WriteLine($"Starting {t}");
        await Task.Delay(500); // Simulate work
        Console.WriteLine($"Finished {t}");
        return t;
    })
    .Subscribe();

await asyncObservable.BroadcastAsync(1);
await asyncObservable.BroadcastAsync(2);

// Output:
// Starting 1
// Finished 1
// Starting 2  (Only starts *after* 1 is finished)
// Finished 2
```

-----

### Delay

`Delay(TimeSpan)` adds a fixed delay before each message is processed. **Note:** This operator must be chained after `InNewThread()`.

```csharp
var asyncObservable = new AsyncObservable<int>();
var subscription = asyncObservable
    .InNewThread() // Required!
    .Delay(TimeSpan.FromSeconds(1)) // Wait 1 second *before* consuming
    .Subscribe(t => Console.WriteLine(t));
    
await asyncObservable.BroadcastAsync(1); // Prints "1" after 1 second
await asyncObservable.BroadcastAsync(2); // Prints "2" after 1 second (2 seconds from start)
```

-----

### WithBuffer

`WithBuffer(int maxBufferLength, Action<Exception>? onError = null)` is a powerful concurrency operator that decouples the broadcaster from the consumer. It places a `Channel` (a high-performance queue) in between.

* If the consumer is slow, broadcasts will quickly fill the buffer.
* If `maxBufferLength` is `0`, the buffer is unbounded.
* If `maxBufferLength` is `> 0`, the broadcaster will `await` (block) if it tries to broadcast to a full buffer, waiting for the consumer to catch up.
* The consumer processes items from the queue on a dedicated background thread.
* `onError` handles exceptions from the consumer without killing the stream.

This is perfect for "fire-and-forget" broadcasts to a slow consumer.

```csharp
var counter = new MessageCounter<int>();
var asyncObservable = new AsyncObservable<int>();

var sub = asyncObservable
    .WithBuffer(5, ex => Console.WriteLine(ex)) // Buffer 5 items
    .MapAsync(async res =>
    {
        await Task.Delay(200); // Very slow consumer
        return res;
    })
    .Subscribe(counter);

Stopwatch watch = new();
watch.Start();

// Broadcast 6 items. The first 5 fill the buffer instantly.
// The 6th broadcast will 'await' until the consumer makes space (after 200ms).
for (var i = 0; i < 6; i++)
{
    await asyncObservable.BroadcastAsync(i); 
}

// This will be fast, as the first 5 broadcasts just queued up.
// The 6th broadcast waited ~200ms, so total time is ~200ms.
Assert.IsGreaterThan(150, watch.ElapsedMilliseconds); 
Assert.AreEqual(0, counter.Count); // Consumer hasn't finished any yet

await Task.Delay(1300); // Wait for (6 items * 200ms) + buffer
Assert.AreEqual(6, counter.Count); // Now the slow consumer is done

sub.Unsubscribe();
```

-----

## 🔌 Utility Operators

### AsyncReflector (Relay)

The `AsyncReflector<T>` class is a special component that is both an `IAsyncObservable` and an `IConsumer`. It's a perfect "relay" or "junction box" that subscribes to one stream and rebroadcasts its messages as a new source. This is useful for forking a stream.

```csharp
var source = new AsyncObservable<int>();
var reflector = new AsyncReflector<int>();

// Subscribe the reflector to the original source
source.Subscribe(reflector);

// Now, multiple consumers can subscribe to the reflector
var sub1 = reflector.Subscribe(t => Console.WriteLine($"Sub 1: {t}"));
var sub2 = reflector.Subscribe(t => Console.WriteLine($"Sub 2: {t}"));

await source.BroadcastAsync(1);
// Output:
// Sub 1: 1
// Sub 2: 1
```

-----

## 💥 Error Handling

How errors are handled depends on the operators you are using.

### 1\. Default Behavior (Fail-Fast)

By default, `BroadcastAsync` uses `Task.WhenAll`. If *any* synchronous part of your pipeline (like a `Filter` or `Map`) or an asynchronous `Consume` method throws an exception, the `Task.WhenAll` will fault. This means the **caller** of `BroadcastAsync` will receive that exception. This is a "fail-fast" model.

```csharp
var asyncObservable = new AsyncObservable<int>();
asyncObservable.Subscribe(t => 
{
    throw new Exception("Consumer failed!");
});

try
{
    // This line will throw an exception.
    await asyncObservable.BroadcastAsync(1);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message); // "Consumer failed!"
}
```

### 2\. Handled Exceptions (`InNewThread` & `WithBuffer`)

Operators like `InNewThread` and `WithBuffer` run the consumer logic on a separate background thread. Because the caller of `BroadcastAsync` is no longer connected, these operators provide an `onError` parameter. This allows you to catch and handle exceptions *without* crashing the broadcast or the app, allowing the stream to continue processing subsequent messages.

```csharp
var counter = new MessageCounter<int>();
var asyncObservable = new AsyncObservable<int>();
Exception? errorReported = null;

asyncObservable.WithBuffer(5, ex => errorReported = ex) // Provide an error handler
    .MapAsync(async res =>
    {
        await Task.Delay(100);
        if (res == 1)
        {
            throw new Exception("Test error!");
        }
        return res;
    })
    .Subscribe(counter); // Attach the final consumer

for (var i = 0; i < 4; i++)
{
    await asyncObservable.BroadcastAsync(i); // 0, 1, 2, 3
}

await Task.Delay(500); // Wait for the buffer to process
Assert.IsNotNull(errorReported); // The error was caught
Assert.AreEqual(3, counter.Count); // Message 1 was skipped, but 0, 2, and 3 were processed
```

-----

## 🧩 Full Example

Here is a complex chain that uses many operators together.

```csharp
int events = 0;
var asyncObservable = new AsyncObservable<int>();
var stage = new MessageStageLast<int[]>(); // Consumer: stage the last item

asyncObservable
    .Throttle(TimeSpan.FromMilliseconds(100)) // 1. Rate-limit
    .Filter(i => i % 2 == 0) // 2. Even numbers only: 0, 2, 4, 6, 8, 10, 12, 14, 16
    .Map(i => i * 100) // 3. Transform: 0, 200, 400, 600, 800, 1000, 1200, 1400, 1600
    .Repeat(2) // 4. Duplicate: 0, 0, 200, 200, 400, 400, 600, 600...
    .Sample(3) // 5. Take 1 of every 3: 200, 400, 800, 1000, 1400, 1600
    .Pipe(_ => events++) // 6. Side-effect: count
    .Aggregate(3) // 7. Batch: [200, 400, 800], [1000, 1400, 1600]
    .Subscribe(stage); // 8. Send to consumer

// Broadcast 18 messages
for (var i = 0; i < 18; i++)
{
    await asyncObservable.BroadcastAsync(i);
}

// Wait for throttle and async operations to complete
await Task.Delay(2000); 

// Analyze the result
Assert.AreEqual(6, events); // Pipe was hit 6 times
Assert.IsNotNull(stage.Stage); // We got a result
Assert.AreEqual(3, stage.Stage!.Length);
Assert.AreEqual(1000, stage.Stage[0]);
Assert.AreEqual(1400, stage.Stage[1]);
Assert.AreEqual(1600, stage.Stage[2]);
```
