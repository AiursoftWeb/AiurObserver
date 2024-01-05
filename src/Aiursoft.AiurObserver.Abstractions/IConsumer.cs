namespace Aiursoft.AiurObserver
{
    public interface IConsumer<in T>
    {
        public Task Consume(T value);
    }
}
