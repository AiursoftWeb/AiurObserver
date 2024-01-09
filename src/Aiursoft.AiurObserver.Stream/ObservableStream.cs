using System.Text;

namespace Aiursoft.AiurObserver.Stream;

public class ObservableStream : AsyncObservable<string>
{
    private readonly System.IO.Stream _stream;

    public ObservableStream(System.IO.Stream stream)
    {
        _stream = stream;
    }

    public async Task ReadToEndAsync()
    {
        var reader = new StreamReader(_stream, Encoding.UTF8);
        while (true)
        {
            if (reader.EndOfStream)
            {
                break;
            }
            
            var line = await reader.ReadLineAsync();
            if (line != null)
            {
                await BroadcastAsync(line);
            }
        }
        
        RemoveAllListeners();
    }
}
