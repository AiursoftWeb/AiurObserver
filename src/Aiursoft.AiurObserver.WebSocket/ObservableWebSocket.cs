using System.Net.WebSockets;
using System.Text;

namespace Aiursoft.AiurObserver.WebSocket;

public class ObservableWebSocket : AsyncObservable<string>, IConsumer<string>
{
    private bool _dropped;
    private readonly System.Net.WebSockets.WebSocket _ws;
    
    // ReSharper disable once UnusedMember.Global
    public bool Connected => !_dropped && _ws.State == WebSocketState.Open;
    
    public ObservableWebSocket(System.Net.WebSockets.WebSocket ws)
    {
        _ws = ws;
    }
    
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public async Task Send(string message, CancellationToken token = default)
    {
        try
        {
            if (_dropped)
            {
                throw new WebSocketException("WebSocket is dropped!");
            }
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            await _ws.SendAsync(buffer, WebSocketMessageType.Text, true, token);
        }
        catch (WebSocketException)
        {
            _dropped = true;
        }
    }

    // ReSharper disable once UnusedMember.Global
    public async Task Listen(CancellationToken token = default)
    {
        try
        {
            var buffer = new ArraySegment<byte>(new byte[4 * 1024]);
            while (true)
            {
                var message = await _ws.ReceiveAsync(buffer, token);
                switch (message.MessageType)
                {
                    case WebSocketMessageType.Text:
                    {
                        var messageBytes = buffer.Skip(buffer.Offset).Take(message.Count).ToArray();
                        var messageString = Encoding.UTF8.GetString(messageBytes);
                        await BroadcastAsync(messageString);
                        break;
                    }
                    case WebSocketMessageType.Close:
                        await _ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Close because of error.",
                            token);
                        _dropped = true;
                        return;
                    case WebSocketMessageType.Binary:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (_ws.State == WebSocketState.Open)
                {
                    continue;
                }

                _dropped = true;
                return;
            }
        }
        catch (WebSocketException)
        {
            // Remote side closed the connection.
            _dropped = true;
        }
        finally
        {
            await Close(token);
        }
    }

    // ReSharper disable once UnusedMember.Global
    public Task Close(CancellationToken token = default)
    {
        if (_ws.State == WebSocketState.Open)
        {
            return _ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Close the connection.", token);
        }

        _dropped = true;
        lock (ObserversEditLock)
        {
            Observers.Clear();
        }

        return Task.CompletedTask;
    }

    public Task Consume (string message)
    {
        return _ws.State == WebSocketState.Open ? Send(message) : Task.CompletedTask;
    }
}