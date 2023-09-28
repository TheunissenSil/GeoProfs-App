using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using GeoProfs_App.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeoProfs_App.Controllers
{
    [Authorize(Roles = "ICT")]
    public class IctController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IctController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Action to list users
        public IActionResult UserList()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        // Action to change user role
        [HttpPost]
        public async Task<IActionResult> ChangeUserRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Remove the user from all roles and assign the new role
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            return RedirectToAction("UserList");
        }

        [HttpPost]
        public IActionResult UpdateWebsite()
        {
            // Logic to update the website here
            // You can use this action to trigger any update process you need
            // For example, you might deploy a new version of the website or update its content

            // Redirect to a success page or return a success message
            return RedirectToAction("WebsiteUpdated");
        }

        // Action to display a success message
        public IActionResult WebsiteUpdated()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserPost()
        {
            string userName = Request.Form["UserName"];
            string email = Request.Form["Email"];
            string password = Request.Form["Password"];
            string confirmPassword = Request.Form["ConfirmPassword"];

            // Now you have the form values, and you can proceed with user creation logic.

            // Example: Create a new ApplicationUser and set its properties
            var user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                // Set other user properties as needed
            };

            // Create the user using UserManager
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // User created successfully
                // You can redirect to a success page or perform other actions
                return RedirectToAction("UserList");
            }

            // If user creation fails, add errors to ModelState and redisplay the form
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            // If the model is not valid or user creation fails, redisplay the form
            return View("CreateUser");
        }





    }
}
