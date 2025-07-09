// Messaging/Services/AzureServiceBusPublisher.cs
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;

namespace VideoTranscoder.VideoTranscoder.Infrastructure.Queues
{
    public class AzureServiceBusPublisherService : IMessageQueueService
    {
        private readonly ServiceBusClient _client;

        public AzureServiceBusPublisherService(ServiceBusClient client)
        {
            _client = client;
        }

        public async Task SendMessageAsync<T>(T message, string queueName)
        {
            var sender = _client.CreateSender(queueName);
            var json = JsonSerializer.Serialize(message);
            var sbMessage = new ServiceBusMessage(json);

            await sender.SendMessageAsync(sbMessage);
        }
    }
}
