using System.Net.WebSockets;

namespace Aiursoft.AiurObserver.WebSocket;

public static class Extensions
{
    public static async Task<ObservableWebSocket> ConnectAsWebSocketServer(this string endpoint)
    {
        var client = new ClientWebSocket();
        await client.ConnectAsync(new Uri(endpoint), CancellationToken.None);
        return new ObservableWebSocket(client);
    }
}