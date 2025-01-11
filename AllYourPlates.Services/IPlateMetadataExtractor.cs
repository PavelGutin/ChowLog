using AllYourPlates.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllYourPlates.Services
{
    public interface IPlateMetadataExtractor
    {
        PlateMetadata ExtractMetadata(Guid plateId, IFormFile file);
    }
}
