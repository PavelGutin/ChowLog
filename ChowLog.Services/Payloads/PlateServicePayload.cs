using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace ChowLog.Services.Payloads
{
    public class PlateServicePayload
    {
        public Guid PlateId { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Description { get; set; }
        public required IdentityUser User { get; set; }
        public IFormFile File { get; set; }
    }
}
