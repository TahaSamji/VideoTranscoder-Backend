using Azure.Messaging.ServiceBus;
using System.Text.Json;
using VideoTranscoder.VideoTranscoder.Application.DTOs;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;

namespace VideoTranscoder.VideoTranscoder.Worker.Services
{
    // Background worker that listens to Service Bus messages and triggers transcoding
    public class TranscodingWorker : BackgroundService
    {
        private readonly ServiceBusProcessor _processor;           // Processor to handle Service Bus messages
        private readonly IServiceProvider _serviceProvider;       // Used for resolving scoped services
        private readonly ILogger<TranscodingWorker> _logger;       // Logger for diagnostics

        public TranscodingWorker(
            ServiceBusProcessor processor,
            IServiceProvider serviceProvider,
            ILogger<TranscodingWorker> logger)
        {
            _processor = processor;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        // This method runs when the worker service starts
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Attach message and error handlers
            _processor.ProcessMessageAsync += ProcessMessageAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;

            // Start listening for messages
            await _processor.StartProcessingAsync(stoppingToken);
            _logger.LogInformation("Transcoding worker started");

            // Keep running until shutdown is requested
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken); // Simple delay to avoid tight loop
            }

            // Stop the processor gracefully
            await _processor.StopProcessingAsync(stoppingToken);
            _logger.LogInformation("Transcoding worker stopped");
        }

        // Called when a message is received from Service Bus
        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            var messageBody = args.Message.Body.ToString(); // Get message body as string
            _logger.LogInformation("Processing message: {MessageId}", args.Message.MessageId);

            try
            {
                // Deserialize message into request object
                var request = JsonSerializer.Deserialize<TranscodeRequestMessage>(messageBody);

                if (request == null)
                {
                    _logger.LogError("Failed to deserialize message: {MessageBody}", messageBody);
                    await args.DeadLetterMessageAsync(args.Message, "InvalidMessageFormat", "Could not deserialize message");
                    return;
                }

                // Create a DI scope so scoped services (like DbContext) are correctly handled
                using var scope = _serviceProvider.CreateScope();
                var transcodingService = scope.ServiceProvider.GetRequiredService<ITranscodingService>();

                // Process the video transcoding
                var result = await transcodingService.TranscodeVideoAsync(request, args.CancellationToken);

                if (result.Equals("Success"))
                {
                    _logger.LogInformation("Successfully processed transcoding for FileId: {FileId}", request.FileId);
                    await args.CompleteMessageAsync(args.Message); // Acknowledge message success
                }
                else
                {
                    _logger.LogError("Transcoding failed for FileId: {FileId} - {Id}", request.FileId, request.EncodingProfileId);

                    // Retry logic: if already tried 3 times, move to dead-letter
                    if (args.Message.DeliveryCount >= 3)
                    {
                        await args.DeadLetterMessageAsync(args.Message, "TranscodingFailed", "result.ErrorMessage");
                    }
                    else
                    {
                        await args.AbandonMessageAsync(args.Message); // Let Service Bus retry
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message: {MessageId}", args.Message.MessageId);

                // On unrecoverable exception, send to dead-letter after retries
                if (args.Message.DeliveryCount >= 3)
                {
                    await args.DeadLetterMessageAsync(args.Message, "ProcessingError", ex.Message);
                }
                else
                {
                    await args.AbandonMessageAsync(args.Message); // Temporary failure; will retry
                }
            }
        }

        // Called when Service Bus encounters an error (e.g., connection failure)
        private async Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Service Bus processing error");
            await Task.CompletedTask; // No-op; can be extended to alerting/monitoring
        }

        // Called when the background service is shutting down
        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Stopping transcoding worker...");

            if (_processor != null)
            {
                await _processor.StopProcessingAsync(stoppingToken); // Gracefully stop
                await _processor.DisposeAsync();                     // Dispose processor resources
            }

            await base.StopAsync(stoppingToken); // Call base implementation
        }
    }
}
