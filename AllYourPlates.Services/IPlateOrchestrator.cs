using AllYourPlates.Services.Payloads;
using AllYourPlates.WebMVC.Models;
using Microsoft.AspNetCore.Identity;

namespace AllYourPlates.Services
{
    public interface IPlateOrchestrator
    {
        Task AddAsync(IEnumerable<PlateServicePayload> platePayload);
        Task<IEnumerable<Plate>> GetAllPlatesAsync(IdentityUser user);
        Task<Plate> GetPlateAsync(Guid id);
        Task DeletePlateAsync(Guid id);
        Task UpdatePlateAsync(Plate plate);
    }
}
