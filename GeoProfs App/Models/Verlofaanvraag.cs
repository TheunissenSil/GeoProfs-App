namespace GeoProfs_App.Models
{
    public class Verlofaanvraag
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string Reason { get; set; }
        public string DifffrentReason { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public ApplicationUser User { get; set; }
    }
}
