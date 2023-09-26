using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace GeoProfs_App.Controllers
{
    [Authorize(Roles = "ICT")]
    public class IctController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
