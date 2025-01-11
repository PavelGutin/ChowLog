using AllYourPlates.Hubs;
using AllYourPlates.Utilities;
using AllYourPlates.WebMVC.DataAccess;
using Azure;
using Azure.AI.Vision.ImageAnalysis;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;


namespace AllYourPlates.Services
{

    public class ImageDescriptionService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ThumbnailProcessingService> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ConcurrentQueue<Guid> _filePaths = new ConcurrentQueue<Guid>();
        private readonly IOptions<ApplicationOptions> _applicationOptions;
        private readonly DirectoryInfo _imagesRoot;

        public ImageDescriptionService(IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<ThumbnailProcessingService> logger,
            IHubContext<NotificationHub> hubContext,
            IOptions<ApplicationOptions> applicationOptions)
        {
            try
            {
                _serviceProvider = serviceProvider;
                _configuration = configuration;
                var scope = _serviceProvider.CreateScope();
                _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                _logger = logger;
                _hubContext = hubContext;
                _applicationOptions = applicationOptions;
                _imagesRoot = new DirectoryInfo(_applicationOptions.Value.ImagesRoot);
            }
            catch (Exception ex)
            {
                _logger.LogError(message: "Error while creating ImageDescriptionService", ex);
                throw;
            }
        }


        public void EnqueueFile(Guid plateId)
        {
            _filePaths.Enqueue(plateId);
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

        private async Task ProcessImage(Guid plateId)
        {
            NotifyClients("Starting to process image " + plateId);

            _logger.LogInformation($"Generating AI description for {plateId}");

            try
            {
                ImageAnalysisClient client = new ImageAnalysisClient(
            new Uri(_configuration["computerVisionEndpoint"]),
            new AzureKeyCredential(_configuration["computerVisionAPIKey"]));


                var platePath = Path.ChangeExtension(
                    Path.Combine(_imagesRoot.FullName, plateId.ToString()),
                    "jpeg");

                // Use a file stream to pass the image data to the analyze call
                using FileStream stream = new FileStream(platePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                //var result = client.Analyze(
                //    BinaryData.FromStream(stream),
                //    VisualFeatures.Caption);

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
                    var plate = await _context.Plate.FindAsync(plateId);
                    if (plate != null)
                    {
                        // Update plate properties as needed
                        plate.Description = result.Caption.Text;
                        _context.Update(plate);
                        await _context.SaveChangesAsync();
                    }
                    NotifyClients("DescriptionGenerated", plateId, plate.Description);

                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "AI ERRORXXXXXXXXXXXXXXXX");
            }
            //return Task.CompletedTask;
        }

        public async Task NotifyClients(string method, params object[] args)
        {
            await _hubContext.Clients.All.SendAsync(method, args);
        }
    }
}
