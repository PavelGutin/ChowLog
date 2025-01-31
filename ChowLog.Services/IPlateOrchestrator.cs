using ChowLog.Services.Payloads;
using ChowLog.WebMVC.Models;
using Microsoft.AspNetCore.Identity;

namespace ChowLog.Services
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
