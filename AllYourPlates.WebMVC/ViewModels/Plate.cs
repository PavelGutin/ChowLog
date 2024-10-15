using System.ComponentModel.DataAnnotations;

namespace AllYourPlates.WebMVC.ViewModels
{
    public class PlateViewModel
    {
        public Guid PlateId { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Description { get; set; }
        public List<IFormFile>? PlateFiles { get; set; }
    }
}