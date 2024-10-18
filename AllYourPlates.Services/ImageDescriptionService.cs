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
    using Azure.AI.Vision.ImageAnalysis;
    using Azure;
    using Microsoft.Extensions.Configuration;

    public class ImageDescriptionService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        public ImageDescriptionService(IConfiguration configuration)
        {
            _configuration = configuration;
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
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    await Task.Delay(1000, stoppingToken); // Delay to avoid busy-waiting
                }
            }
        }

        private Task ProcessImage(string filePath)
        {
            var thumbnailPath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + "_thmb.jpeg");
            var thumbnailSize = new Size(200, 200);




            ImageAnalysisClient client = new ImageAnalysisClient(
                new Uri(_configuration["computerVisionEndpoint"]),
                new AzureKeyCredential(_configuration["computerVisionAPIKey"]));


            //call the API here

            return Task.CompletedTask;
        }
    }
}
