using ChowLog.Common;
using Microsoft.AspNetCore.Http;

namespace ChowLog.Services
{
    public interface IPlateMetadataExtractor
    {
        PlateMetadata ExtractMetadata(Guid plateId, IFormFile file);
    }
}
