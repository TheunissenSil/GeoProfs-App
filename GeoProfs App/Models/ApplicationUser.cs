using Microsoft.AspNetCore.Identity;

namespace GeoProfs_App.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int verlofsaldo { get; set; }
    }
}
