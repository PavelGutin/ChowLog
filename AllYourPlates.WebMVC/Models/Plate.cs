using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AllYourPlates.WebMVC.Models
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
