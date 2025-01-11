using AllYourPlates.Common;
using AllYourPlates.Hubs;
using AllYourPlates.Utilities;
using AllYourPlates.WebMVC.DataAccess;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
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

        private readonly IConfiguration _configuration;
        private readonly ConcurrentQueue<Guid> _plates = new ConcurrentQueue<Guid>();
        private readonly DirectoryInfo _imagesRoot;
        private readonly ILogger<ThumbnailProcessingService> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IOptions<ApplicationOptions> _applicationOptions;
        private readonly ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;

        public PlateMetadataService(ILogger<ThumbnailProcessingService> logger,
            IHubContext<NotificationHub> hubContext,
            IConfiguration configuration,
            IOptions<ApplicationOptions> applicationOptions,
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _hubContext = hubContext;
            _configuration = configuration;
            _applicationOptions = applicationOptions;
            var scope = _serviceProvider.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _imagesRoot = new DirectoryInfo(_applicationOptions.Value.ImagesRoot);
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
            
            var plate = await _context.Plate.FindAsync(plateId);
            if (plate != null)
            {
                // Update plate properties as needed
                plate.Timestamp = timeTaken;
                _context.Update(plate);
                await _context.SaveChangesAsync();
            }

            NotifyClients("MetadataExtracted", plateId.ToString() + timeTaken.ToString());
            //}
            return new PlateMetadata { TimeTaken = timeTaken };
        }

        public async Task NotifyClients(string method, string message)
        {
            await _hubContext.Clients.All.SendAsync(method, message);
        }

    }
}
