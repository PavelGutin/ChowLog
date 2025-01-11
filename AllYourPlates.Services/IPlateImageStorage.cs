using Microsoft.AspNetCore.Http;

namespace AllYourPlates.Services
{
    public interface IPlateImageStorage
    {
        Task SaveImage(Guid plateId, IFormFile file);
    }
}