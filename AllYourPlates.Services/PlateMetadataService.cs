using AllYourPlates.Common;
using AllYourPlates.Hubs;
using AllYourPlates.Utilities;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace AllYourPlates.Services
{
    //TODO this and other backround services could benefit from a base class
    public class PlateMetadataService : BackgroundService
    {
        private readonly ConcurrentQueue<Guid> _plates = new ConcurrentQueue<Guid>();
        private readonly DirectoryInfo _imagesRoot;
        private readonly ILogger<ThumbnailProcessingService> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPlateService _plateService;
        private readonly IOptions<ApplicationOptions> _applicationOptions;

        public PlateMetadataService(ILogger<ThumbnailProcessingService> logger,
            IHubContext<NotificationHub> hubContext,
            IServiceProvider serviceProvider,
            IOptions<ApplicationOptions> applicationOptions)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _hubContext = hubContext;
            var scope = _serviceProvider.CreateScope();
            //TODO environment variable should be a fallback. The value should be in a configuration file

            _applicationOptions = applicationOptions;
            _imagesRoot = new DirectoryInfo($"{_applicationOptions.Value.DataPath}/Plates");
            _plateService = scope.ServiceProvider.GetRequiredService<IPlateService>();
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
                        await ExtractMetadata(plate);
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


        private async Task<PlateMetadata> ExtractMetadata(Guid plateId)
        {
            var platePath = Path.ChangeExtension(
                                Path.Combine(_imagesRoot.FullName, plateId.ToString()),
                                "jpeg");
            DateTime timeTaken = DateTime.Now;
            var metadata = ImageMetadataReader.ReadMetadata(platePath);

            var dateTaken = metadata.OfType<ExifSubIfdDirectory>()
                .FirstOrDefault()?.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal);

            if (dateTaken.HasValue)
            {
                timeTaken = dateTaken.Value;
            }

            var plate = await _plateService.GetPlateAsync(plateId);

            if (plate != null)
            {
                plate.Timestamp = timeTaken;
                await _plateService.UpdatePlateAsync(plate);
            }
            else
            {
                _logger.LogError("Plate not found");
                throw new Exception("Plate not found while trying to update metadata");
            }

            NotifyClients("MetadataExtracted", plateId.ToString() + timeTaken.ToString());

            return new PlateMetadata { TimeTaken = timeTaken };
        }

        public async Task NotifyClients(string method, string message)
        {
            _hubContext.Clients.All.SendAsync(method, message);
        }

    }
}
