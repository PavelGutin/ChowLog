using AllYourPlates.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text;


namespace AllYourPlates.Services
{

    public class ImageDescriptionDummyService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ThumbnailProcessingService> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ConcurrentQueue<Guid> _filePaths = new();
        private readonly IPlateService _plateService;

        public ImageDescriptionDummyService(IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<ThumbnailProcessingService> logger,
            IHubContext<NotificationHub> hubContext)
        {
            try
            {
                _serviceProvider = serviceProvider;
                var scope = _serviceProvider.CreateScope();
                _logger = logger;
                _hubContext = hubContext;
                _plateService = scope.ServiceProvider.GetRequiredService<IPlateService>();
            }
            catch (Exception ex)
            {
                _logger.LogError(message: "Error while creating ImageDescriptionDummyService", ex);
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

            _logger.LogInformation($"Generating Dummy description for {plateId}");

            try
            {
                var random = new Random();
                var sb = new StringBuilder();

                for (int i = 0; i < random.Next(20,30); i++)
                {
                    if (random.Next(0, 5) == 0)
                    {
                        sb.Append(' ');
                    }
                    else
                    {
                        sb.Append((char) random.Next('a', 'z' + 1));
                    }
                }

                var plate = await _plateService.GetPlateAsync(plateId);

                if (plate != null)
                {
                    plate.Description = sb.ToString();
                    await _plateService.UpdatePlateAsync(plate);
                }
                NotifyClients("DescriptionGenerated", plateId, plate.Description);
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
