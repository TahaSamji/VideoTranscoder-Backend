// Messaging/Services/AzureServiceBusPublisher.cs
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;

namespace VideoTranscoder.VideoTranscoder.Worker.QueuePublisher
{
    public class AzureServiceBusPublisher : IMessageQueueService
    {
        private readonly ServiceBusClient _client;

        public AzureServiceBusPublisher(ServiceBusClient client)
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
