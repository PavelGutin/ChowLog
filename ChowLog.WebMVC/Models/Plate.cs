using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ChowLog.WebMVC.Models
{
    public class Plate
    {
        [Key] 
        public Guid PlateId { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Description { get; set; }
        public IdentityUser User { get; set; } 
    }
}
