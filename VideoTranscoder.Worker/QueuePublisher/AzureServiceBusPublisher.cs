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
        public async Task SendBatchAsync<T>(IEnumerable<T> messages, string queueName)
        {
            var sender = _client.CreateSender(queueName);
            var batch = await sender.CreateMessageBatchAsync();

            foreach (var msg in messages)
            {
                var json = JsonSerializer.Serialize(msg);
                var sbMessage = new ServiceBusMessage(json);

                if (!batch.TryAddMessage(sbMessage))
                {
                    // Send current full batch
                    await sender.SendMessagesAsync(batch);

                    // Start a new batch
                    batch = await sender.CreateMessageBatchAsync();

                    // Retry adding the current message
                    if (!batch.TryAddMessage(sbMessage))
                        throw new InvalidOperationException("❌ Message too large to fit in an empty batch.");
                }
            }

            // Send the last batch if it has any messages
            if (batch.Count > 0)
            {
                await sender.SendMessagesAsync(batch);
            }
        }
    }
}
