using GeoProfs_App.Data;
using GeoProfs_App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace GeoProfs_App.Controllers
{
    [Authorize]
    public class VerlofaanvragenController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GeoProfs_AppContext mvcGeoProfs_AppContext;

        public VerlofaanvragenController(UserManager<ApplicationUser> userManager, GeoProfs_AppContext mvcGeoProfs_AppContext)
        {
            _userManager = userManager;
            this.mvcGeoProfs_AppContext = mvcGeoProfs_AppContext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var viewModel = new VerlofaanvraagViewModel
            {
                Verlofaanvragen = mvcGeoProfs_AppContext.Verlofaanvragen.ToList(),
                Verlofaanvraag = new Verlofaanvraag()
            };

            return View(viewModel);
        }

        [HttpPost]
        // Create
        public async Task<IActionResult> Add(VerlofaanvraagViewModel viewModel)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Index");
            }

            var diffrentReason = viewModel.Verlofaanvraag.DifffrentReason;
            if (diffrentReason == null)
            {
                diffrentReason = "null";
            }

            var verlofaanvraag = new Verlofaanvraag()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Reason = viewModel.Verlofaanvraag.Reason,
                DifffrentReason = diffrentReason,
                StartDate = viewModel.Verlofaanvraag.StartDate,
                EndDate = viewModel.Verlofaanvraag.EndDate,
                Status = "pending",
            };

            await mvcGeoProfs_AppContext.Verlofaanvragen.AddAsync(verlofaanvraag);
            await mvcGeoProfs_AppContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Delete
        public async Task<IActionResult> Delete(Guid id)
        {
            var verlofaanvraag = await mvcGeoProfs_AppContext.Verlofaanvragen.FindAsync(id);
            if (verlofaanvraag == null)
            {
                // Verlofaanvraag not found
                return NotFound();
            }

            mvcGeoProfs_AppContext.Verlofaanvragen.Remove(verlofaanvraag);
            await mvcGeoProfs_AppContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}