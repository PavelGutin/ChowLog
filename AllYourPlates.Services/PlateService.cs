using AllYourPlates.Services.Payloads;
using AllYourPlates.WebMVC.DataAccess;
using AllYourPlates.WebMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AllYourPlates.Services
{
    public class PlateService : IPlateService
    {
        //private readonly ApplicationDbContext _dbContext;
        private readonly ThumbnailProcessingService _thumbnailService;
        private readonly ImageDescriptionService _imageDescriptionService;
        private readonly PlateMetadataService _plateMetadataService;
        private readonly IPlateImageStorage _plateImageStorage;
        private readonly IPlateRepsitory _plateRepository;

        public PlateService(/*ApplicationDbContext dbContext, */
            ThumbnailProcessingService thumbnailService,
            ImageDescriptionService imageDescriptionService, 
            PlateMetadataService plateMetadataService,
            IPlateRepsitory plateRepository,
            IPlateImageStorage plateImageStorage)
        {
            //_dbContext = dbContext;
            _thumbnailService = thumbnailService;
            _imageDescriptionService = imageDescriptionService;
            _plateRepository = plateRepository;
            _plateImageStorage = plateImageStorage;
            _plateMetadataService = plateMetadataService;
        }

        public async Task AddAsync(IEnumerable<PlateServicePayload> plates)
        {
            foreach (var item in plates)
            {
                var newPlate = new Plate 
                {
                    PlateId = item.PlateId,
                    User = item.User
                };

                await _plateRepository.AddPlateAsync(newPlate);
                _plateImageStorage.SaveImage(newPlate.PlateId, item.File);
                _plateMetadataService.EnqueueFile(newPlate.PlateId);
                _thumbnailService.EnqueueFile(newPlate.PlateId);
                _imageDescriptionService.EnqueueFile(newPlate.PlateId);
            }
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
    }
}
