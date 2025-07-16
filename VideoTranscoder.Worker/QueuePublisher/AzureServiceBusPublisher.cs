using System.Text.Json;
using Azure.Messaging.ServiceBus;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;

namespace VideoTranscoder.VideoTranscoder.Worker.QueuePublisher
{
    /// <summary>
    /// Handles publishing messages to Azure Service Bus queues.
    /// </summary>
    public class AzureServiceBusPublisher : IMessageQueueService
    {
        private readonly ServiceBusClient _client;

        public AzureServiceBusPublisher(ServiceBusClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Sends a single message to the specified Azure Service Bus queue.
        /// </summary>
        /// <typeparam name="T">The type of the message object.</typeparam>
        /// <param name="message">The message object to send.</param>
        /// <param name="queueName">The name of the queue to send to.</param>
        public async Task SendMessageAsync<T>(T message, string queueName)
        {
            // Create a sender for the given queue
            var sender = _client.CreateSender(queueName);

            // Serialize the message to JSON
            var json = JsonSerializer.Serialize(message);

            // Wrap the JSON in a ServiceBusMessage
            var sbMessage = new ServiceBusMessage(json);

            // Send the message to the queue
            await sender.SendMessageAsync(sbMessage);
        }

        /// <summary>
        /// Sends a batch of messages to the specified Azure Service Bus queue.
        /// Automatically splits into multiple batches if needed.
        /// </summary>
        /// <typeparam name="T">The type of message objects.</typeparam>
        /// <param name="messages">The list of messages to send.</param>
        /// <param name="queueName">The target queue name.</param>
        public async Task SendBatchAsync<T>(IEnumerable<T> messages, string queueName)
        {
            // Create a sender for the given queue
            var sender = _client.CreateSender(queueName);

            // Create an initial empty message batch
            var batch = await sender.CreateMessageBatchAsync();

            foreach (var msg in messages)
            {
                // Serialize each message to JSON
                var json = JsonSerializer.Serialize(msg);
                var sbMessage = new ServiceBusMessage(json);

                // Try to add the message to the current batch
                if (!batch.TryAddMessage(sbMessage))
                {
                    // Current batch is full — send it
                    await sender.SendMessagesAsync(batch);

                    // Start a new batch
                    batch = await sender.CreateMessageBatchAsync();

                    // Retry adding the current message to the new batch
                    if (!batch.TryAddMessage(sbMessage))
                    {
                        // If it still doesn't fit, it's too large for a batch
                        throw new InvalidOperationException("❌ Message too large to fit in an empty batch.");
                    }
                }
            }

            // Send any remaining messages in the last batch
            if (batch.Count > 0)
            {
                await sender.SendMessagesAsync(batch);
            }
        }
    }
}
