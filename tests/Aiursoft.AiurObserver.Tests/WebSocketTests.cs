using Aiursoft.AiurObserver.DefaultConsumers;
using Aiursoft.AiurObserver.WebSocket;
using Aiursoft.AiurObserver.WebSocket.Server;
using Aiursoft.CSTools.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable StringLiteralTypo

namespace Aiursoft.AiurObserver.Tests;

[TestClass]
public class WebSocketTests
{
    [TestMethod]
    public async Task TestWebSocket()
    {
        var port = Network.GetAvailablePort();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
        var app = builder.Build();
        app.UseWebSockets();
        app.Use(async (HttpContext context, RequestDelegate _) =>
        {
            var client = await context.AcceptWebSocketClient();
            client
                .Filter(t => t == "ping")
                .Map(_ => "pong")
                .Subscribe(client);

            await client.Listen(context.RequestAborted);
        });

        await app.StartAsync();

        var client = await $"ws://localhost:{port}/".ConnectAsWebSocketServer();

        var count = new MessageCounter<string>();
        client.Subscribe(count);

        var stage = new MessageStageLast<string>();
        client.Subscribe(stage);

        await Task.Factory.StartNew(() => client.Listen());

        await client.Send("ping");
        await client.Send("aaaaa");
        await client.Send("bbbbb");
        await client.Send("ccccc");
        await client.Send("ping");

        await Task.Delay(300);
        Assert.AreEqual(2, count.Count);
        Assert.AreEqual("pong", stage.Stage);

        await client.Close();
    }

    [TestMethod]
    public async Task TestClientDisconnect()
    {
        var port = Network.GetAvailablePort();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
        var app = builder.Build();
        app.UseWebSockets();
        app.Use(async (HttpContext context, RequestDelegate _) =>
        {
            var client = await context.AcceptWebSocketClient();
            client.Subscribe(new MessageCounter<string>());
            await client.Listen(context.RequestAborted);
        });

        await app.StartAsync();

        var client = await $"ws://localhost:{port}/".ConnectAsWebSocketServer();
        await Task.Factory.StartNew(() => client.Listen());

        await client.Send("ping");
        await client.Close(); // Client closes the connection

        await Task.Delay(100); // Give server some time to process

        Assert.IsFalse(client.Connected); // Client should be disconnected
    }

    [TestMethod]
    public async Task TestServerDisconnect()
    {
        var port = Network.GetAvailablePort();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
        var app = builder.Build();
        app.UseWebSockets();
        app.Use(async (HttpContext context, RequestDelegate _) =>
        {
            var client = await context.AcceptWebSocketClient();
            await client.Send("server message"); // Initial message from server
            await Task.Delay(100); // Simulate some processing time
            await client.Close(); // Server closes the connection
        });

        await app.StartAsync();

        var client = await $"ws://localhost:{port}/".ConnectAsWebSocketServer();
        var receivedMessages = new List<string>();
        client.Subscribe(m =>
        {
            receivedMessages.Add(m);
            return Task.CompletedTask;
        });

        await Task.Factory.StartNew(() => client.Listen());

        await Task.Delay(200); // Wait to ensure server closes the connection

        Assert.IsFalse(client.Connected); // Client should be disconnected gracefully
        Assert.IsTrue(receivedMessages.Contains("server message")); // Verify message reception before disconnect
    }

    [TestMethod]
    public async Task TestServerReflection()
    {
        var port = Network.GetAvailablePort();
        var builder = WebApplication.CreateBuilder();
        var reflector = new AsyncReflector<string>();
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
        var app = builder.Build();
        app.UseWebSockets();
        app.Use(async (HttpContext context, RequestDelegate _) =>
        {
            var ws = await context.AcceptWebSocketClient();
            reflector.Subscribe(ws);
            ws.Subscribe(reflector);
            await ws.Listen(context.RequestAborted);
        });

        await app.StartAsync();
        
        var client = await $"ws://localhost:{port}/".ConnectAsWebSocketServer();
        var count = new MessageCounter<string>();
        client.Subscribe(count);
        await Task.Factory.StartNew(() => client.Listen());
        
        var client2 = await $"ws://localhost:{port}/".ConnectAsWebSocketServer();   
        var count2 = new MessageCounter<string>();  
        client2.Subscribe(count2);
        await Task.Factory.StartNew(() => client2.Listen());
        
        Parallel.For(0, 10, _ =>
        {
            for (var j = 0; j < 100 * 500; j++)
            {
                var task1 = client.Send("ping");
                var task2 = client2.Send("pong");
                Task.WhenAll(task1, task2).Wait();
            }
        });
        
        // 10 * 100 * 500 * 2 = 100 * 100 * 100
        
        await Task.Delay(2000); 
        Assert.AreEqual(100 * 100 * 100, count.Count);
        Assert.AreEqual(100 * 100 * 100, count2.Count);
        
        await client.Close();
        Assert.IsFalse(client.Connected);
    }
    
    [TestMethod]
    public async Task TestServerSuddenShutdown()
    {
        var port = Network.GetAvailablePort();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
        var app = builder.Build();
        app.UseWebSockets();
        app.Use(async (HttpContext context, RequestDelegate _) =>
        {
            var client = await context.AcceptWebSocketClient();
            await client.Listen(context.RequestAborted);
        });

        await app.StartAsync();

        var client = await $"ws://localhost:{port}/".ConnectAsWebSocketServer();
        await Task.Factory.StartNew(() => client.Listen());

        await client.Send("ping");
        await app.StopAsync();

        await Task.Delay(100); // 等待处理
        Assert.IsFalse(client.Connected); // 客户端应断开连接
    }
    
    [TestMethod]
    public async Task TestHighConcurrentClients()
    {
        var port = Network.GetAvailablePort();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
        var app = builder.Build();
        app.UseWebSockets();
        var counter = new MessageCounter<string>();
        app.Use(async (HttpContext context, RequestDelegate _) =>
        {
            var client = await context.AcceptWebSocketClient();
            client.Subscribe(counter);
            await client.Listen(context.RequestAborted);
        });

        await app.StartAsync();

        var tasks = new List<Task>();
        for (var i = 0; i < 1000; i++) // 1000 个并发客户端
        {
            tasks.Add(Task.Run(async () =>
            {
                var client = await $"ws://localhost:{port}/".ConnectAsWebSocketServer();
                await Task.Factory.StartNew(() => client.Listen());
                await client.Send("test1");
                await client.Send("test2");
                await client.Close();
            }));
        }

        await Task.WhenAll(tasks);
        await Task.Delay(2000);
        
        Assert.AreEqual(2000, counter.Count);
    }
    
    [TestMethod]
    public async Task TestClientMidConnectionDisconnect()
    {
        var port = Network.GetAvailablePort();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
        var app = builder.Build();
        app.UseWebSockets();
        app.Use(async (HttpContext context, RequestDelegate _) =>
        {
            var client = await context.AcceptWebSocketClient();
            var messageCount = new MessageCounter<string>();
            client.Subscribe(messageCount);

            await client.Listen(context.RequestAborted);
            Assert.IsTrue(messageCount.Count > 0); // 确认在断开前收到至少一个消息
        });

        await app.StartAsync();

        var client = await $"ws://localhost:{port}/".ConnectAsWebSocketServer();
        await Task.Factory.StartNew(() => client.Listen());

        // 发送几条消息
        await client.Send("message 1");
        await client.Send("message 2");

        // 模拟客户端中途断开
        await client.Close();

        await Task.Delay(100); // 给服务器一些时间来处理断开事件

        Assert.IsFalse(client.Connected); // 验证客户端确实已断开
    }

}