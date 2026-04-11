# 🌐 WebSocket Deep Dive with AiurObserver

AiurObserver turns WebSockets into first-class observable streams. Because `ObservableWebSocket` implements both `IAsyncObservable<string>` and `IConsumer<string>`, it can be both a source of data and a destination for data.

This allows you to build complex communication patterns using simple, composable operators.

---

## 🧭 Scenarios

- [The Echo Pattern (Ping-Pong)](#-the-echo-pattern-ping-pong)
- [The Reflector Pattern (Chat/Bridge)](#-the-reflector-pattern-chatbridge)
- [The Collector Pattern (Centralized Logging)](#-the-collector-pattern-centralized-logging)
- [The Broadcaster Pattern (Status Updates)](#-the-broadcaster-pattern-status-updates)
- [The Filtered Proxy Pattern](#-the-filtered-proxy-pattern)

---

## 🏓 The Echo Pattern (Ping-Pong)

The simplest use case: reacting to a message and sending a response back to the same client.

```csharp
app.Use(async (context, next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var ws = await context.AcceptWebSocketClient();

        // ws is an Observable (incoming) AND a Consumer (outgoing)
        ws.Filter(msg => msg == "ping")
          .Map(_ => "pong")
          .Subscribe(ws); // Subscribe the socket to its own processed stream

        await ws.Listen();
    }
});
```

---

## 🪞 The Reflector Pattern (Chat/Bridge)

Use an `AsyncReflector` to create a bridge where any message sent by one client is automatically rebroadcast to all other clients.

```csharp
// Define a central reflector (junction box)
var chatRoom = new AsyncReflector<string>();

app.Use(async (context, next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var ws = await context.AcceptWebSocketClient();

        // 1. When the chat room has a message, send it to this client
        var sub1 = chatRoom.Subscribe(ws);

        // 2. When this client sends a message, push it into the chat room
        var sub2 = ws.Subscribe(chatRoom);

        try 
        {
            await ws.Listen();
        }
        finally 
        {
            sub1.Unsubscribe();
            sub2.Unsubscribe();
        }
    }
});
```

---

## 📥 The Collector Pattern (Centralized Logging)

Collect messages from many different WebSocket clients into a single processing unit (like a database writer or a counter).

```csharp
var messageLog = new MessageCounter<string>(); // Or any IConsumer<string>

app.Use(async (context, next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var ws = await context.AcceptWebSocketClient();

        // Every client subscribes the central log to its incoming stream
        ws.Subscribe(messageLog);

        await ws.Listen();
        // messageLog.Count now includes messages from this client
    }
});
```

---

## 📢 The Broadcaster Pattern (Status Updates)

Broadcast a single source of truth (like a system clock, a CPU monitor, or a game state) to all connected clients.

```csharp
// A central source of data
var systemEvents = new AsyncObservable<string>();

app.Use(async (context, next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var ws = await context.AcceptWebSocketClient();

        // Subscribe the client to the central event source
        using var sub = systemEvents.Subscribe(ws);

        await ws.Listen();
    }
});

// Elsewhere in your app:
await systemEvents.BroadcastAsync("System update: Version 2.0 deployed!");
```

---

## 🛡️ The Filtered Proxy Pattern

Combine operators to create intelligent proxies. In this example, we bridge two clients but throttle the messages and filter out sensitive data.

```csharp
var internalSource = new AsyncObservable<string>();
var externalClient = await "ws://external-api.com".ConnectAsWebSocketServer();

internalSource
    .Throttle(TimeSpan.FromMilliseconds(500)) // Rate limit
    .Filter(msg => !msg.Contains("PASSWORD")) // Security filter
    .Map(msg => $"[INTERNAL] {msg}")          // Metadata tagging
    .Subscribe(externalClient);               // Forward to external

await internalSource.BroadcastAsync("PASSWORD=123"); // Dropped
await internalSource.BroadcastAsync("Hello!");       // Sent as "[INTERNAL] Hello!"
```

---

## 🚀 Advanced Composition

Because these are all standard `IAsyncObservable` and `IConsumer` objects, you can mix and match them with other extensions.

**Example: Stream a command's output to a WebSocket with buffering.**

```csharp
var runner = new LongCommandRunner(logger);
var ws = await context.AcceptWebSocketClient();

runner.Output
    .WithBuffer(100) // Buffer if the network is slow
    .Map(line => $"[LOG] {line}")
    .Subscribe(ws);

await runner.Run("dotnet", "watch run", ".");
```
