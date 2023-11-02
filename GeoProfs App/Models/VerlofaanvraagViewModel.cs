using System.ComponentModel.DataAnnotations;

namespace GeoProfs_App.Models
{
    public class VerlofaanvraagViewModel
    {
        public List<Verlofaanvraag> Verlofaanvragen { get; set; }
        public Verlofaanvraag Verlofaanvraag { get; set; }
        public List<ApplicationUser> Users { get; set; }
    }
}
