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
            while (true)
            {
                var messageBuffer = new List<byte>();
                WebSocketReceiveResult message;

                do
                {
                    var buffer = new ArraySegment<byte>(new byte[4 * 1024]);
                    message = await _ws.ReceiveAsync(buffer, token);

                    switch (message.MessageType)
                    {
                        case WebSocketMessageType.Text:
                            messageBuffer.AddRange(buffer.Array!.Skip(buffer.Offset).Take(message.Count));
                            break;
                        case WebSocketMessageType.Close:
                            await _ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closed by client.", token);
                            _dropped = true;
                            return;
                        case WebSocketMessageType.Binary:
                            // Handle binary messages if needed
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                } while (!message.EndOfMessage);

                var messageString = Encoding.UTF8.GetString(messageBuffer.ToArray());
                await BroadcastAsync(messageString);

                if (_ws.State != WebSocketState.Open)
                {
                    _dropped = true;
                    return;
                }
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