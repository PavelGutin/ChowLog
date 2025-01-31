using ChowLog.WebMVC.Models;
using Microsoft.AspNetCore.Identity;

namespace ChowLog.Services
{
    public interface IPlateRepsitory
    {
        Task AddPlateAsync(Plate newPlate);
        Task<IEnumerable<Plate>> GetAllPlatesAsync(IdentityUser user);
        Task<Plate> GetPlateAsync(Guid plate);
        Task DeletePlateAsync(Guid id);

        Task UpdatePlateAsync(Plate plate);
    }
}