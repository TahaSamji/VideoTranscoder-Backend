
using Azure.Messaging.ServiceBus;
using System.Text.Json;
using VideoTranscoder.VideoTranscoder.Application.DTOs;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;


namespace VideoTranscoder.VideoTranscoder.Worker.Services
{
    public class TranscodingWorker : BackgroundService
    {
        private readonly ServiceBusProcessor _processor;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TranscodingWorker> _logger;

        public TranscodingWorker(
            ServiceBusProcessor processor,
            IServiceProvider serviceProvider,
            ILogger<TranscodingWorker> logger)
        {
            _processor = processor;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _processor.ProcessMessageAsync += ProcessMessageAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;

            await _processor.StartProcessingAsync(stoppingToken);

            _logger.LogInformation("Transcoding worker started");

            // Keep the service running until cancellation is requested
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }

            await _processor.StopProcessingAsync(stoppingToken);
            _logger.LogInformation("Transcoding worker stopped");
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            var messageBody = args.Message.Body.ToString();
            
            _logger.LogInformation("Processing message: {MessageId}", args.Message.MessageId);

            try
            {
                var request = JsonSerializer.Deserialize<TranscodeRequestMessage>(messageBody);
                
                if (request == null)
                {
                    _logger.LogError("Failed to deserialize message: {MessageBody}", messageBody);
                    await args.DeadLetterMessageAsync(args.Message, "InvalidMessageFormat", "Could not deserialize message");
                    return;
                }

                // Create a new scope for each message to ensure proper DI lifecycle
                using var scope = _serviceProvider.CreateScope();
                var transcodingService = scope.ServiceProvider.GetRequiredService<ITranscodingService>();

                var result = await transcodingService.TranscodeVideoAsync(request, args.CancellationToken);

                if (result.Equals("Success"))
                {
                    _logger.LogInformation("Successfully processed transcoding for FileId: {FileId}", request.FileId);
                    await args.CompleteMessageAsync(args.Message);
                }
                else
                {
                    _logger.LogError("Transcoding failed for FileId: {FileId} - {Error}", request.FileId, request.EncodingProfileId);
                    
                    // Retry logic - check delivery count
                    if (args.Message.DeliveryCount >= 3)
                    {
                        await args.DeadLetterMessageAsync(args.Message, "TranscodingFailed", "result.ErrorMessage");
                    }
                    else
                    {
                        await args.AbandonMessageAsync(args.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message: {MessageId}", args.Message.MessageId);
                
                if (args.Message.DeliveryCount >= 3)
                {
                    await args.DeadLetterMessageAsync(args.Message, "ProcessingError", ex.Message);
                }
                else
                {
                    await args.AbandonMessageAsync(args.Message);
                }
            }
        }

        private async Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Service Bus processing error");
            await Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Stopping transcoding worker...");
            
            if (_processor != null)
            {
                await _processor.StopProcessingAsync(stoppingToken);
                await _processor.DisposeAsync();
            }

            await base.StopAsync(stoppingToken);
        }
    }
}
