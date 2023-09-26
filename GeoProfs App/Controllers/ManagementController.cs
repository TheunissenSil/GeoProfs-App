using GeoProfs_App.Data;
using GeoProfs_App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace GeoProfs_App.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagementController : Controller
    {
        private readonly GeoProfs_AppContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public ManagementController(GeoProfs_AppContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        // Return view
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var verlofaanvragen = await _dbContext.Verlofaanvragen
                .Include(v => v.User)
                .OrderBy(v => v.StartDate)
                .ToListAsync();

            ViewBag.Users = _dbContext.Users
                .Select(u => new
                {
                    u.UserName,
                    u.Email,
                    u.verlofsaldo,
                    Role = _dbContext.UserRoles
                        .Where(ur => ur.UserId == u.Id)
                        .Join(_dbContext.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                        .FirstOrDefault()
                })
                .ToList();

            return View(verlofaanvragen);
        }

        // Update status verlofaanvraag
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(Guid id, string status)
        {
            // Kijk of verlofaanvraag bestaat
            var verlofaanvraag = await _dbContext.Verlofaanvragen.FindAsync(id);
            if (verlofaanvraag == null)
            {
                return NotFound();
            }

            // Als die wordt goedgekeurd haal het verlofsaldo weg
            if (status == "Approved")
            {
                // Bereken duration
                var duration = (verlofaanvraag.EndDate - verlofaanvraag.StartDate).Days;

                // Pak de user en kijk of die bsetaat
                var user = await _userManager.FindByIdAsync(verlofaanvraag.UserId);
                if (user == null)
                {
                    return NotFound();
                }

                // Remove verlofsaldo
                user.verlofsaldo -= duration;
                await _userManager.UpdateAsync(user);
            }

            // Update de status
            verlofaanvraag.Status = status;
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }


        // Update verslofsaldo
        [HttpPost]
        public async Task<IActionResult> UpdateVerlofsaldo(string userName, int verlofsaldo)
        {
            // Check of user bestaat
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return NotFound();
            }

            // Update het verlofsaldo
            user.verlofsaldo = verlofsaldo;
            await _userManager.UpdateAsync(user);

            return NoContent();
        }

        // Update user's role
        [HttpPost]
        public async Task<IActionResult> UpdateRole(string userName, string role)
        {
            // Kijk of user bestaat
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return NotFound();
            }

            // Update de rol van user
            await _userManager.RemoveFromRolesAsync(user, _userManager.GetRolesAsync(user).Result.ToArray());
            await _userManager.AddToRoleAsync(user, role);

            return NoContent();
        }

        // Delete user
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userName)
        {
            // Check of user bestaat
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return NotFound();
            }

            // Delete de user 
            await _userManager.DeleteAsync(user);

            return NoContent();
        }
    }
}
