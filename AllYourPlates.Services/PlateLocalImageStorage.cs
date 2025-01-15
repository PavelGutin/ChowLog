using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;

namespace AllYourPlates.Services
{
    public class PlateLocalImageStorage : IPlateImageStorage
    {
        public async Task SaveImage(Guid plateId, IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();
            var newFileName = Path.ChangeExtension(plateId.ToString(), ".jpeg");
            //TODO this need to be moved to a configuration file
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/plates", newFileName);

            if (extension != ".jpeg" && extension != ".jpg")
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                using var image = Image.Load(memoryStream);
                await image.SaveAsJpegAsync(filePath);
            }
            else
            {
                using var fileStream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(fileStream);
            }
        }
    }
}
