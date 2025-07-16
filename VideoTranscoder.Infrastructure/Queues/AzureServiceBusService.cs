using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;

namespace VideoTranscoder.VideoTranscoder.Infrastructure.Queues
{
    public class AzureServiceBusPublisherService : IMessageQueueService
    {
        private readonly ServiceBusClient _client;
        private readonly ILogger<AzureServiceBusPublisherService> _logger;

        public AzureServiceBusPublisherService(ServiceBusClient client, ILogger<AzureServiceBusPublisherService> logger)
        {
            _client = client;
            _logger = logger;
        }

        /// <summary>
        /// Sends a single message to the specified Azure Service Bus queue.
        /// </summary>
        public async Task SendMessageAsync<T>(T message, string queueName)
        {
            var sender = _client.CreateSender(queueName);
            var json = JsonSerializer.Serialize(message);
            var sbMessage = new ServiceBusMessage(json);

            _logger.LogInformation("📤 Sending single message to queue '{Queue}'", queueName);
            await sender.SendMessageAsync(sbMessage);
            _logger.LogInformation("✅ Message sent to queue '{Queue}'", queueName);
        }

        /// <summary>
        /// Sends multiple messages in batches to the specified Azure Service Bus queue.
        /// </summary>
        public async Task SendBatchAsync<T>(IEnumerable<T> messages, string queueName)
        {
            var sender = _client.CreateSender(queueName);
            var batch = await sender.CreateMessageBatchAsync();
            int totalSent = 0;
            int batchCount = 0;

            foreach (var msg in messages)
            {
                var json = JsonSerializer.Serialize(msg);
                var sbMessage = new ServiceBusMessage(json);

                // Try to add message to current batch
                if (!batch.TryAddMessage(sbMessage))
                {
                    // If batch is full, send it
                    await sender.SendMessagesAsync(batch);
                    _logger.LogInformation("✅ Sent batch #{BatchNumber} to queue '{Queue}' with {Count} messages", ++batchCount, queueName, batch.Count);
                    totalSent += batch.Count;

                    // Start a new batch
                    batch = await sender.CreateMessageBatchAsync();

                    // Retry adding current message
                    if (!batch.TryAddMessage(sbMessage))
                    {
                        throw new InvalidOperationException("❌ Message too large to fit in an empty batch.");
                    }
                }
            }

            // Send the final batch if it has any messages
            if (batch.Count > 0)
            {
                await sender.SendMessagesAsync(batch);
                _logger.LogInformation("✅ Sent final batch #{BatchNumber} to queue '{Queue}' with {Count} messages", ++batchCount, queueName, batch.Count);
                totalSent += batch.Count;
            }

            _logger.LogInformation("📦 Total {Count} messages sent in {BatchCount} batches to queue '{Queue}'", totalSent, batchCount, queueName);
        }
    }
}
