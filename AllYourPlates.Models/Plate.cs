using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AllYourPlates.WebMVC.Models
{
    public class Plate
    {
        [Key] 
        public required Guid PlateId { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Description { get; set; }
        public required IdentityUser User { get; set; } 
    }
}
