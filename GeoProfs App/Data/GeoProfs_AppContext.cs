using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GeoProfs_App.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace GeoProfs_App.Data
{
    public class GeoProfs_AppContext : IdentityDbContext<ApplicationUser>
    {
        public GeoProfs_AppContext (DbContextOptions<GeoProfs_AppContext> options)
            : base(options)
        {
        }
    }
}
