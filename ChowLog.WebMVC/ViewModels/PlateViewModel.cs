namespace ChowLog.WebMVC.ViewModels
{
    public class PlateViewModel
    {
        public Guid PlateId { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Description { get; set; }
        public string? Thumbnail { get; set; }
        public string ImageUrl { get; set; }
    }
}
