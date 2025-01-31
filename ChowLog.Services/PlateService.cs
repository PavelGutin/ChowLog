using ChowLog.WebMVC.Models;
using Microsoft.AspNetCore.Identity;

namespace ChowLog.Services
{
    public class PlateService : IPlateService
    {
        private readonly IPlateRepsitory _plateRepository;

        public PlateService(IPlateRepsitory plateRepository)
        {
            _plateRepository = plateRepository;
        }

        public async Task AddAsync(IEnumerable<Plate> plates)
        {
            foreach (var plate in plates)
            {
                await _plateRepository.AddPlateAsync(plate);
            }
        }

        public async Task AddAsync(Plate plate)
        {
            await _plateRepository.AddPlateAsync(plate);
        }

        public async Task DeletePlateAsync(Guid id)
        {
            await _plateRepository.DeletePlateAsync(id);
        }

        public async Task<IEnumerable<Plate>> GetAllPlatesAsync(IdentityUser user)
        {
            return await _plateRepository.GetAllPlatesAsync(user); 
        }

        public async Task<Plate> GetPlateAsync(Guid id)
        {
            return await _plateRepository.GetPlateAsync(id);
        }
        public async Task UpdatePlateAsync(Plate plate)
        {
            await _plateRepository.UpdatePlateAsync(plate);
        }
    }
}
