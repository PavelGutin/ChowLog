using AllYourPlates.Common;
using Microsoft.AspNetCore.Http;

namespace AllYourPlates.Services
{
    public interface IPlateMetadataExtractor
    {
        PlateMetadata ExtractMetadata(Guid plateId, IFormFile file);
    }
}
