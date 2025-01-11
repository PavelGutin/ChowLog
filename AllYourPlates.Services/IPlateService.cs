using AllYourPlates.Services.Payloads;
using AllYourPlates.WebMVC.Models;
using Microsoft.AspNetCore.Identity;

//TODO move this to a common project
namespace AllYourPlates.Services
{
    public interface IPlateService
    {
        Task AddAsync(IEnumerable<PlateServicePayload> platePayload);
        Task<IEnumerable<Plate>> GetAllPlatesAsync(IdentityUser user);
        Task<Plate> GetPlateAsync(Guid id);
        Task DeletePlateAsync(Guid id);

    }
}
