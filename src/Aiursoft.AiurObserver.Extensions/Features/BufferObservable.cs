using System.Threading.Channels;

namespace Aiursoft.AiurObserver.Features;

public class BufferObservable<T>(IAsyncObservable<T> source, int maxBufferLength, Action<Exception>? onError)
    : IAsyncObservable<T>
{
    private readonly Action<Exception> _onError = onError ?? Console.Error.WriteLine;
    public int MaxBufferLength { get; } = maxBufferLength;

    public ISubscription Subscribe(IConsumer<T> observer)
    {
        var channel = MaxBufferLength == 0
            ? Channel.CreateUnbounded<T>()
            : Channel.CreateBounded<T>(new BoundedChannelOptions(MaxBufferLength)
            {
                FullMode = BoundedChannelFullMode.Wait
            });
        var subscription = source.Subscribe(new Consumer<T>(async value =>
        {
            await channel.Writer.WriteAsync(value);
        }));
        Task.Run(async () =>
        {
            await foreach (var value in channel.Reader.ReadAllAsync())
            {
                try
                {
                    await observer.Consume(value);
                }
                catch (Exception ex)
                {
                    _onError(ex);
                }
            }
        });

        return new Subscription(() =>
        {
            subscription.Unsubscribe();
            // Complete the channel. This will make the consumerTask complete after consumed all item in the queue.
            channel.Writer.Complete();
        });
    }
}
