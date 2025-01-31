using Microsoft.AspNetCore.Http;

namespace ChowLog.Services
{
    public interface IPlateImageStorage
    {
        Task SaveImage(Guid plateId, IFormFile file);
    }
}