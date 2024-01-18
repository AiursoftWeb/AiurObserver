using Microsoft.AspNetCore.Http;

namespace Aiursoft.AiurObserver.WebSocket.Server;

public static class Extensions
{
    public static async Task<ObservableWebSocket> AcceptWebSocketClient(this HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            throw new InvalidOperationException("This request is not a WebSocket request!");
        }
        return new ObservableWebSocket(await context.WebSockets.AcceptWebSocketAsync());
    }
}
