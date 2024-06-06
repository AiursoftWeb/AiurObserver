namespace Aiursoft.AiurObserver
{
    public class Consumer<T>(Func<T, Task> onNext) : IConsumer<T>
    {
        public Task Consume(T value)
        {
            return onNext(value);
        }
    }
}
