using AllYourPlates.WebMVC.Models;

namespace AllYourPlates.WebMVC.ViewModels
{
    public class GroupedPlatesViewModel
    {
        public DateTime Date { get; set; }
        public List<Plate> Plates { get; set; }
    }
}
