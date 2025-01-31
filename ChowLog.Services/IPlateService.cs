using ChowLog.Services.Payloads;
using ChowLog.WebMVC.Models;
using Microsoft.AspNetCore.Identity;

//TODO move this to a common project
namespace ChowLog.Services
{
    public interface IPlateService
    {
        Task AddAsync(Plate platePayload);
        Task AddAsync(IEnumerable<Plate> plates);
        Task<IEnumerable<Plate>> GetAllPlatesAsync(IdentityUser user);
        Task<Plate> GetPlateAsync(Guid id);
        Task DeletePlateAsync(Guid id);
        Task UpdatePlateAsync(Plate plate);

    }
}
