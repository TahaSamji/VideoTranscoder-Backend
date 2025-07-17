namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface IMessageQueueService
    {
        // Sends a single message of type T to the specified Azure Service Bus queue.
        Task SendMessageAsync<T>(T message, string queueName);


        // Sends a batch of messages of type T to the specified Azure Service Bus queue.
        Task SendBatchAsync<T>(IEnumerable<T> messages, string queueName);
    }
}
