using ChowLog.Hubs;
using ChowLog.Utilities;
using Azure;
using Azure.AI.Vision.ImageAnalysis;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text;


namespace ChowLog.Services
{

    public class ImageDescriptionService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ThumbnailProcessingService> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ConcurrentQueue<Guid> _filePaths = new();
        private readonly IOptions<ApplicationOptions> _applicationOptions;
        private readonly DirectoryInfo _imagesRoot;
        private readonly IPlateService _plateService;

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
                _logger = logger;
                _hubContext = hubContext;
                _applicationOptions = applicationOptions;
                _imagesRoot = new DirectoryInfo($"{_applicationOptions.Value.DataPath}/Plates");
                _plateService = scope.ServiceProvider.GetRequiredService<IPlateService>();
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

                ImageAnalysisResult result = client.Analyze(BinaryData.FromStream(stream), VisualFeatures.Caption);

                if (result.Caption.Text != null)
                {
                    var plate = await _plateService.GetPlateAsync(plateId);

                    if (plate != null)
                    {
                        plate.Description = result.Caption.Text;
                        await _plateService.UpdatePlateAsync(plate);
                    }
                    NotifyClients("DescriptionGenerated", plateId, plate.Description);
                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "AI error");
            }
            //return Task.CompletedTask;
        }

        public async Task NotifyClients(string method, params object[] args)
        {
            await _hubContext.Clients.All.SendAsync(method, args);
        }
    }
}
