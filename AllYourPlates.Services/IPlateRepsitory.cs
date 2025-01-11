using AllYourPlates.WebMVC.Models;
using Microsoft.AspNetCore.Identity;

namespace AllYourPlates.Services
{
    public interface IPlateRepsitory
    {
        Task AddPlateAsync(Plate newPlate);
        Task<IEnumerable<Plate>> GetAllPlatesAsync(IdentityUser user);
        Task<Plate> GetPlateAsync(Guid plate);
        Task DeletePlateAsync(Guid id);
    }
}