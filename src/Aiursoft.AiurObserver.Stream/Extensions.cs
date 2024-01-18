namespace Aiursoft.AiurObserver.Stream;

public static class Extensions
{
    public static ObservableStream ToObservableStream(this System.IO.Stream stream)
    {
        return new ObservableStream(stream);
    }
}