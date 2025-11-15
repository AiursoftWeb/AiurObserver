using System.Text;

namespace Aiursoft.AiurObserver.Stream;

public class ObservableStream : AsyncObservable<string>
{
    private readonly System.IO.Stream _stream;

    public ObservableStream(System.IO.Stream stream)
    {
        _stream = stream;
    }

    public async Task ReadToEndAsync(CancellationToken token = default)
    {
        var reader = new StreamReader(_stream, Encoding.UTF8);

        while (await reader.ReadLineAsync(token) is { } line)
        {
            await BroadcastAsync(line);
        }

        RemoveAllListeners();
    }
}
