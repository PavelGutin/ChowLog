namespace AllYourPlates.Services
{
    using Microsoft.Extensions.Hosting;
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;
    using SixLabors.ImageSharp.Formats.Jpeg;
    using Microsoft.Extensions.Logging;

    public class ThumbnailProcessingService : BackgroundService
    {
        private readonly ConcurrentQueue<string> _filePaths = new ConcurrentQueue<string>();
        
        private readonly ILogger<ThumbnailProcessingService> _logger;
        public ThumbnailProcessingService(ILogger<ThumbnailProcessingService> logger)
        {
            _logger = logger;
        }
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
                        await ProcessThumbnail(filePath);
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

        private async Task ProcessThumbnail(string filePath)
        {
            _logger.LogInformation($"Generating thumbnail for {filePath}");
            var thumbnailPath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + "_thmb.jpeg");
            var thumbnailSize = new Size(200, 200);

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var image = Image.Load(stream))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = thumbnailSize,
                    Mode = ResizeMode.Max
                }));
                await image.SaveAsync(thumbnailPath, new JpegEncoder());
            }

            //return Task.CompletedTask;
        }
    }
}
