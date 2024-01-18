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
        var app =builder.Build();
        app.UseWebSockets();
        app.Use(async (HttpContext context, RequestDelegate _) =>
        {
            ISubscription? subscription = null;
            try
            {
                var client = await context.AcceptWebSocketClient();
                subscription = client
                    .Filter(t => t == "ping")
                    .Map(_ => "pong")
                    .Subscribe(client);

                await client.Listen(context.RequestAborted);
            }
            finally
            {
                subscription?.Unsubscribe();
            }
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
}
