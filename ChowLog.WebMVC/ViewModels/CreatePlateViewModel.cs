using System.ComponentModel.DataAnnotations;

namespace ChowLog.WebMVC.ViewModels
{
    public class CreatePlateViewModel
    {
        public Guid PlateId { get; set; }
        //public DateTime Timestamp { get; set; }
        //public string? Description { get; set; }
        public List<IFormFile>? PlateFiles { get; set; }
    }
}