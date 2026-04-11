# 💻 Running Commands with AiurObserver

The `Aiursoft.AiurObserver.Command` package turns shell execution into an observable stream. It allows you to run any external program (like `ping`, `dotnet`, `git`, or custom scripts) and react to every line written to `STDOUT` or `STDERR` as it happens.

---

## 🧭 Scenarios

- [Real-time Process Monitoring](#-real-time-process-monitoring)
- [Remote Terminal (Command-to-WebSocket)](#-remote-terminal-command-to-websocket)
- [Multi-Command Pipeline](#-multi-command-pipeline)
- [Build Failure Alert System](#-build-failure-alert-system)

---

## 🔍 Real-time Process Monitoring

The most common use case: run a long-running command and process its output line-by-line.

```csharp
var runner = new LongCommandRunner(logger);

// Observe standard output
runner.Output
    .Filter(line => line.Contains("Reply from"))
    .Subscribe(line => Console.WriteLine($"Ping: {line}"));

// Observe error output
runner.Error.Subscribe(line => Console.Error.WriteLine($"[ERROR] {line}"));

// Run ping 4 times
await runner.Run("ping", "-c 4 google.com", ".");
```

---

## 🌐 Remote Terminal (Command-to-WebSocket)

This powerful pattern allows you to stream a local process's output directly to a web browser via WebSockets.

```csharp
var runner = new LongCommandRunner(logger);
var ws = await context.AcceptWebSocketClient();

// Pipe every line of the command output to the WebSocket
runner.Output.Subscribe(ws);
runner.Error.Map(err => $"[STDERR] {err}").Subscribe(ws);

// Run a build command
await runner.Run("dotnet", "build", ".");
```

---

## ⛓️ Multi-Command Pipeline

You can use `AsyncReflector` to bridge multiple command runners together.

```csharp
var centralLog = new AsyncReflector<string>();
var runner1 = new LongCommandRunner(logger);
var runner2 = new LongCommandRunner(logger);

// Both runners feed into the same central log
runner1.Output.Subscribe(centralLog);
runner2.Output.Subscribe(centralLog);

// A single subscriber listens to both
centralLog.Subscribe(line => Console.WriteLine($"Combined Log: {line}"));

await Task.WhenAll(
    runner1.Run("ls", "-la", "."),
    runner2.Run("whoami", "", ".")
);
```

---

## 🛡️ Build Failure Alert System

Combine `Command` with `Filter` and `SampleLast` to capture only the relevant failure information.

```csharp
var runner = new LongCommandRunner(logger);
var errors = new MessageCounter<string>();
var lastError = runner.Error.StageLast();

runner.Error.Subscribe(errors);

await runner.Run("npm", "run build", ".");

if (errors.Count > 0)
{
    Console.WriteLine($"Build failed with {errors.Count} errors.");
    Console.WriteLine($"Final error reported: {lastError.Stage}");
}
```

---

## 🛠️ Combined: Clock + Command (Uptime Dashboard)

**Example: Run a status command every minute and broadcast it to all connected WebSocket clients.**

```csharp
var clock = new ObservableClock(TimeSpan.FromMinutes(1));
var runner = new LongCommandRunner(logger);
var broadcaster = new AsyncReflector<string>();

// 1. When the clock ticks, run the command
clock.Subscribe(async _ => 
{
    await runner.Run("uptime", "", ".");
});

// 2. Feed the command output into the broadcaster
runner.Output.Subscribe(broadcaster);

// 3. Any WebSocket client can subscribe to the broadcaster
app.Use(async (context, next) => {
    var ws = await context.AcceptWebSocketClient();
    using var sub = broadcaster.Subscribe(ws);
    await ws.Listen();
});

_ = clock.StartClock();
```
