using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Connections;

namespace Aiursoft.AiurObserver.WebSocket;

public class ObservableWebSocket : AsyncObservable<string>, IConsumer<string>
{
    private bool _dropped;
    private readonly System.Net.WebSockets.WebSocket _ws;
    
    public bool Connected => !_dropped && _ws.State == WebSocketState.Open;
    
    public ObservableWebSocket(System.Net.WebSockets.WebSocket ws)
    {
        if (ws.State != WebSocketState.Open)
        {
            throw new WebSocketException("WebSocket is not open! Please first `Accept` or `Connect` the WebSocket.");
        }
        _ws = ws;
    }
    
    public async Task Send(string message, CancellationToken token = default)
    {
        try
        {
            if (!Connected)
            {
                throw new WebSocketException("WebSocket is not connected!");
            }

            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            await _ws.SendAsync(buffer, WebSocketMessageType.Text, true, token);
        }
        catch (WebSocketException)
        {
            _dropped = true;
            await Close(token);
        }
        catch (TaskCanceledException)
        {
            _dropped = true;
            await Close(token);
        }
        catch (ConnectionAbortedException)
        {
            _dropped = true;
            await Close(token);
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
            // Ignore. This happens when the client closes the connection.
        }
        catch (TaskCanceledException)
        {
            // Ignore. This happens when the client closes the connection.
        }
        catch (ConnectionAbortedException)
        {
            // Ignore. This happens when the client closes the connection.
        }
        finally
        {
            _dropped = true;
            await Close(token);
        }
    }

    // ReSharper disable once UnusedMember.Global
    public Task Close(CancellationToken token = default)
    {
        _dropped = true;
        lock (ObserversEditLock)
        {
            Observers.Clear();
        }

        if (_ws.State == WebSocketState.Open)
        {
            if (!token.IsCancellationRequested) 
            {
                return _ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Close the connection.", token);
            }
            else
            {
                return _ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Close the connection.", CancellationToken.None);
            }
        }

        return Task.CompletedTask;
    }

    public Task Consume(string message)
    {
        return Connected ? Send(message) : Task.CompletedTask;
    }
}