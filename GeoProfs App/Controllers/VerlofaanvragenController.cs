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
    // Moet ingelogd zijn
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

        // Lijst van verlofaanvragen en view retun
        [HttpGet]
        public IActionResult Index()
        {
            var viewModel = new VerlofaanvraagViewModel
            {
                Verlofaanvragen = mvcGeoProfs_AppContext.Verlofaanvragen.OrderBy(v => v.StartDate).ToList(),
                Verlofaanvraag = new Verlofaanvraag()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Add(VerlofaanvraagViewModel viewModel)
        {
            // Check if userId exists
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Index");
            }

            // Kijk of de user genoeg verlofsaldo heeft
            if (viewModel.Verlofaanvraag.Reason != "Ziek")
            {
                // Get user
                var user = await _userManager.FindByIdAsync(userId);
                var verlofsaldo = user.verlofsaldo;

                // Bereken de datums
                var startDate = viewModel.Verlofaanvraag.StartDate.Date;
                var endDate = viewModel.Verlofaanvraag.EndDate.Date;
                var duration = (int)(endDate - startDate).TotalDays + 1;

                // Kijk of verlofsaldo genoeg is
                if (verlofsaldo < duration)
                {
                    ModelState.AddModelError("", "Je hebt niet genoeg verlofsaldo.");
                    var updatedViewModel = new VerlofaanvraagViewModel
                    {
                        Verlofaanvragen = mvcGeoProfs_AppContext.Verlofaanvragen.OrderBy(v => v.StartDate).ToList(),
                        Verlofaanvraag = viewModel.Verlofaanvraag
                    };
                    return View("Index", updatedViewModel);
                }
            }

            // Check if there is a different reason filled in
            var diffrentReason = viewModel.Verlofaanvraag.DifffrentReason;
            if (diffrentReason == null)
            {
                diffrentReason = "null";
            }

            // Check if the user is "ziek"
            var status = "pending";
            if (viewModel.Verlofaanvraag.Reason == "Ziek")
            {
                status = "ziek";
            }

            // Create a new verlofaanvraag
            var verlofaanvraag = new Verlofaanvraag()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Reason = viewModel.Verlofaanvraag.Reason,
                DifffrentReason = diffrentReason,
                StartDate = viewModel.Verlofaanvraag.StartDate,
                EndDate = viewModel.Verlofaanvraag.EndDate,
                Status = status,
            };

            // Gooi in de database
            await mvcGeoProfs_AppContext.Verlofaanvragen.AddAsync(verlofaanvraag);
            await mvcGeoProfs_AppContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Delete
        public async Task<IActionResult> Delete(Guid id)
        {
            // Check of verlofaanvraag bestaat
            var verlofaanvraag = await mvcGeoProfs_AppContext.Verlofaanvragen.FindAsync(id);
            if (verlofaanvraag == null)
            {
                return NotFound();
            }

            // Verwijder verlofaanvraag
            mvcGeoProfs_AppContext.Verlofaanvragen.Remove(verlofaanvraag);
            await mvcGeoProfs_AppContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}