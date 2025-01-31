using AllYourPlates.Hubs;
using AllYourPlates.Utilities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Collections.Concurrent;

namespace AllYourPlates.Services
{


    //TODO maybe using a tupple here is a bad idea, but creating a class just seems wrong 
    public class ThumbnailProcessingService : BackgroundService
    {
        //private readonly IConfiguration _configuration;
        private readonly ConcurrentQueue<Guid> _plates = new();
        private readonly DirectoryInfo _imagesRoot;
        private readonly ILogger<ThumbnailProcessingService> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IOptions<ApplicationOptions> _applicationOptions;

        public ThumbnailProcessingService(ILogger<ThumbnailProcessingService> logger,
            IHubContext<NotificationHub> hubContext,
            IConfiguration configuration,
            IOptions<ApplicationOptions> applicationOptions)
        {
            _logger = logger;
            _hubContext = hubContext;
            _applicationOptions = applicationOptions;
            _imagesRoot = new DirectoryInfo($"{_applicationOptions.Value.DataPath}/Plates");
        }
        public void EnqueueFile(Guid plateId)
        {
            _plates.Enqueue(plateId);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_plates.TryDequeue(out var plate))
                {
                    try
                    {
                        await GenerateThumbnail(plate);
                    }
                    catch (Exception ex)
                    {
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

        private async Task GenerateThumbnail(Guid plateId)
        {
            var platePath = Path.ChangeExtension(
                                Path.Combine(_imagesRoot.FullName, plateId.ToString()),
                                "jpeg");

            _logger.LogInformation($"Generating thumbnail for {platePath}");
            var thumbnailPath = Path.Combine(Path.GetDirectoryName(platePath), Path.GetFileNameWithoutExtension(platePath) + "_thmb.jpeg");
            var thumbnailSize = new Size(200, 200);

            using (var stream = new FileStream(platePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var image = Image.Load(stream))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = thumbnailSize,
                    Mode = ResizeMode.Max
                }));
                await image.SaveAsync(thumbnailPath, new JpegEncoder());
            }

            NotifyClients("ThumbnailGenerated", plateId.ToString());

            //return Task.CompletedTask;
        }

        public async Task NotifyClients(string method, string message)
        {
            _hubContext.Clients.All.SendAsync(method, message);
        }
    }
}
