using AllYourPlates.WebMVC.DataAccess;
using AllYourPlates.WebMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AllYourPlates.Services
{
    public class PlateLocalDBRepository : IPlateRepsitory
    {
        private readonly ApplicationDbContext _dbContext;
        public PlateLocalDBRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public Task AddPlateAsync(Plate newPlate)
        {
            _dbContext.Plate.Add(newPlate);
            return _dbContext.SaveChangesAsync();
        }

        //TODO add null checks
        public async Task DeletePlateAsync(Guid id)
        {
            var plate = _dbContext.Plate.FirstOrDefault(p => p.PlateId == id);
            _dbContext.Plate.Remove(plate);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Plate>> GetAllPlatesAsync(IdentityUser user)
        {
            return  await _dbContext.Plate
                .Where(p => p.User == user)
                .ToListAsync();
        }

        public async Task<Plate> GetPlateAsync(Guid plate)
        {
            return await _dbContext.Plate.FirstOrDefaultAsync(p => p.PlateId == plate);
        }

        public async Task UpdatePlateAsync(Plate plate)
        {
            // Ensure the Plate exists in the database
            var existingPlate = await _dbContext.Plate.FirstOrDefaultAsync(p => p.PlateId == plate.PlateId);

            if (existingPlate == null)
            {
                throw new KeyNotFoundException($"Plate with ID {plate.PlateId} not found.");
            }

            // Update the fields
            _dbContext.Entry(existingPlate).CurrentValues.SetValues(plate);

            // Save changes to the database
            await _dbContext.SaveChangesAsync();
        }
    }
}
