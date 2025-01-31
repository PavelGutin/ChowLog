using ChowLog.DataAccess;
using ChowLog.WebMVC.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;

namespace ChowLog.Services
{
    public class PlateMongoRepository : IPlateRepsitory
    {
        private readonly IMongoCollection<Plate> _plates;

        public PlateMongoRepository(IMongoDbContext dbContext)
        {
            _plates = dbContext.GetCollection<Plate>("plates");
        }

        public async Task AddPlateAsync(Plate newPlate)
        {
            await _plates.InsertOneAsync(newPlate);
        }

        public async Task DeletePlateAsync(Guid id)
        {
            await _plates.DeleteOneAsync(p => p.PlateId == id);
        }

        public async Task<IEnumerable<Plate>> GetAllPlatesAsync(IdentityUser user)
        {
            //var filter = Builders<Plate>.Filter.Eq(p => p.User, user);
            //var result = await _plates.Find(filter).ToListAsync();

            var result = await _plates.Find(p => p.User == user ).ToListAsync();

            return result;
        }

        public async Task<Plate> GetPlateAsync(Guid plate)
        {
            return await _plates.FindAsync(p => p.PlateId == plate)
                .Result.FirstOrDefaultAsync();
        }


        public async Task UpdatePlateAsync(Plate updatedPlate)
        {
            // Set the filter to find the plate with the specified ID
            //var filter = Builders<Plate>.Filter.Eq(p => p.PlateId, plateId);

            // Replace the existing plate with the updated one
            try
            {

                var filter = Builders<Plate>.Filter.Eq(p => p.PlateId, updatedPlate.PlateId);
                var result = await _plates.ReplaceOneAsync(filter, updatedPlate);

                //var result = await _plates.ReplaceOneAsync(p => updatedPlate.PlateId == updatedPlate.PlateId, updatedPlate);
                // Optional: Check if the update was acknowledged
                if (!result.IsAcknowledged || result.ModifiedCount == 0)
                {
                    throw new Exception($"Failed to update the plate with ID {updatedPlate.PlateId}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update the plate with ID {updatedPlate.PlateId}", ex);
            }


        }


    }
}
