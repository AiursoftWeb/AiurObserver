# ⏰ Time & Scheduling with AiurObserver Clock

The `Aiursoft.AiurObserver.Clock` package provides a simple but powerful way to turn "time" into an observable stream. This is ideal for background tasks, UI heartbeats, or any logic that needs to trigger periodically.

---

## 🧭 Scenarios

- [The Simple Ticker](#-the-simple-ticker)
- [Scheduled Task Trigger](#-scheduled-task-trigger)
- [UI Update Throttler (Minutes/Hours)](#-ui-update-throttler-minuteshours)
- [The Precision Heartbeat](#-the-precision-heartbeat)

---

## ⏲️ The Simple Ticker

The most basic usage: emit an event every second.

```csharp
// 1. Create a clock with 1-second interval
var clock = new ObservableClock(TimeSpan.FromSeconds(1));

// 2. Subscribe to the "ticks"
using var sub = clock.Subscribe(now => 
{
    Console.WriteLine($"Tick: {now:HH:mm:ss}");
    return Task.CompletedTask;
});

// 3. Start the clock (this loop runs until cancelled)
var cts = new CancellationTokenSource();
await clock.StartClock(cts.Token);
```

---

## 📅 Scheduled Task Trigger

You can use standard LINQ operators to turn a high-frequency clock into a specific scheduled trigger.

**Example: Run a cleanup task every day at 3:00 AM.**

```csharp
var clock = new ObservableClock(TimeSpan.FromMinutes(1));

clock.Filter(now => now.Hour == 3 && now.Minute == 0) // Only at 3:00 AM
     .Subscribe(async _ => 
     {
         await CleanDatabaseAsync();
         Console.WriteLine("Database cleaned!");
     });

await clock.StartClock();
```

---

## 📉 UI Update Throttler (Minutes/Hours)

Sometimes you want a clock to run fast internally for precision, but only update your UI or log files occasionally. Use the `Sample` operator to achieve this.

```csharp
var clock = new ObservableClock(TimeSpan.FromSeconds(1));

clock.Sample(60) // Only let 1 tick through every 60 ticks (once per minute)
     .Subscribe(now => 
     {
         UpdateLastSeenLabel(now);
     });

_ = clock.StartClock(); // Fire and forget in background
```

---

## 💓 The Precision Heartbeat

By combining `Clock` with `InNewThread` and `WithBuffer`, you can ensure that your clock remains accurate even if one specific tick's consumer is slow.

```csharp
var clock = new ObservableClock(TimeSpan.FromSeconds(1));

clock.InNewThread()   // Process ticks on a background thread
     .WithBuffer(10)  // Buffer ticks if the consumer hangs briefly
     .Subscribe(async now => 
     {
         await LongRunningAuditAsync(); // This won't block the next 1-second tick
     });

await clock.StartClock();
```

---

## 🛠️ Integration: Clock + Command

**Example: Run a "Status" command every 5 seconds and broadcast the result.**

```csharp
var clock = new ObservableClock(TimeSpan.FromSeconds(5));
var runner = new LongCommandRunner(logger);

clock.Subscribe(async _ => 
{
    // Every 5 seconds, run a command
    await runner.Run("uptime", "", ".");
});

// Listen to the results of that command
runner.Output.Subscribe(line => Console.WriteLine($"System Uptime: {line}"));

await clock.StartClock();
```
