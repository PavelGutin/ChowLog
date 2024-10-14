using System.ComponentModel.DataAnnotations;

namespace AllYourPlates.WebMVC.Models
{
    public class Plate
    {
        [Key] 
        public Guid PlateId { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Description { get; set; }
    }
}
