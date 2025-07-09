
namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface IMessageQueueService
    {
        Task SendMessageAsync<T>(T message, string queueName);
    }
}