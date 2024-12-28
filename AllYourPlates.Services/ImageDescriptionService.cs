namespace AllYourPlates.Services
{
    using AllYourPlates.Hubs;
    using AllYourPlates.WebMVC.DataAccess;
    using Azure;
    using Azure.AI.Vision.ImageAnalysis;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class ImageDescriptionService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ThumbnailProcessingService> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public ImageDescriptionService(IServiceProvider serviceProvider, 
            IConfiguration configuration, 
            ILogger<ThumbnailProcessingService> logger, 
            IHubContext<NotificationHub> hubContext)
        {
            try
            {
                _serviceProvider = serviceProvider;
                _configuration = configuration;
                var scope = _serviceProvider.CreateScope();
                _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                _logger = logger;
                _hubContext = hubContext;
            }
            catch (Exception ex)
            {
                _logger.LogError(message: "Error while creating ImageDescriptionService", ex);
                throw;
            }
        }
        private readonly ConcurrentQueue<string> _filePaths = new ConcurrentQueue<string>();

        public void EnqueueFile(string filePath)
        {
            _filePaths.Enqueue(filePath);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_filePaths.TryDequeue(out var filePath))
                {
                    try
                    {
                        await ProcessImage(filePath);
                    }
                    catch (Exception ex)
                    {
                        // Handle exception (log it, etc.)
                        _logger.LogError(ex.Message);
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    await Task.Delay(5000, stoppingToken); // Delay to avoid busy-waiting
                }
            }
        }

        private async Task ProcessImage(string filePath)
        {
            NotifyClients("Starting to process image " + filePath);

            _logger.LogInformation($"Generating AI description for {filePath}");
            var plateId = Path.GetFileNameWithoutExtension(filePath);
            try
            {
                ImageAnalysisClient client = new ImageAnalysisClient(
            new Uri(_configuration["computerVisionEndpoint"]),
            new AzureKeyCredential(_configuration["computerVisionAPIKey"]));

                // Use a file stream to pass the image data to the analyze call
                using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                

                ImageAnalysisResult result = client.Analyze(
                    BinaryData.FromStream(stream),
                    VisualFeatures.Caption);

                // Display analysis results
                // Get image captions
                if (result.Caption.Text != null)
                {
                    Console.WriteLine(" Caption:");
                    Console.WriteLine($"   \"{result.Caption.Text}\", Confidence {result.Caption.Confidence:0.00}\n");



                    // Update the Plate object
                    var plate = await _context.Plate.FindAsync(Guid.Parse(plateId));
                    if (plate != null)
                    {
                        // Update plate properties as needed
                        plate.Description = result.Caption.Text;
                        _context.Update(plate);
                        await _context.SaveChangesAsync();
                    }

                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "AI ERRORXXXXXXXXXXXXXXXX");
            }
            NotifyClients("Done processing image " + filePath);
            NotifyClients("PlateProcessed", plateId);

            //return Task.CompletedTask;
        }

        public async Task NotifyClients(string message)
        {
            // Notify all clients connected to the NotificationHub
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
        }
        public async Task NotifyClients(string method, string message)
        {
            await _hubContext.Clients.All.SendAsync(method, message);
        }
    }
}
