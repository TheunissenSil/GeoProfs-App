using GeoProfs_App.Data;
using GeoProfs_App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.Data;
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;
using MySql.Data.MySqlClient;

namespace GeoProfs_App.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagementController : Controller
    {
        private readonly GeoProfs_AppContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly string _connectionString = "Server=localhost;Port=3306;Database=GeoProfs;Uid=root;";

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

        // Make rapport
        public IActionResult DownloadRapport()
        {
            // Fetch data from SQL table (Verlofaanvragen)
            DataTable dataTable = GetDataFromSQL();

            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                // Create an Excel package
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Rapport");

                    // Create a new DataTable with desired column order
                    DataTable newDataTable = new DataTable();
                    newDataTable.Columns.Add("UserName");
                    newDataTable.Columns.Add("Reason");
                    newDataTable.Columns.Add("StartDate");
                    newDataTable.Columns.Add("EndDate");
                    newDataTable.Columns.Add("Status");

                    foreach (DataRow row in dataTable.Rows)
                    {
                        newDataTable.Rows.Add(
                            row["UserName"],
                            row["Reason"],
                            row["StartDate"],
                            row["EndDate"],
                            row["Status"]
                        );
                    }

                    // Load the new DataTable into the Excel worksheet
                    worksheet.Cells["A1"].LoadFromDataTable(newDataTable, true);

                    using (var range = worksheet.Cells[2, 3, newDataTable.Rows.Count + 1, 4]) 
                    {
                        range.Style.Numberformat.Format = "yyyy-MM-dd"; 
                    }

                    worksheet.Cells.AutoFitColumns();
                    worksheet.Cells["A1:E1"].Style.Font.Bold = true;

                    // Set content type and disposition for the response
                    Response.Headers.Add("Content-Disposition", "attachment; filename=Rapport.xlsx");
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                    // Write the Excel package to the response stream
                    Response.Body.Write(package.GetAsByteArray());
                }
            }
            else
            {
                return Content("No data to export." + dataTable.Rows.Count);
            }

            return new EmptyResult();
        }

        private DataTable GetDataFromSQL()
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))  
                {
                    connection.Open();

                    // SQL query 
                    string query = "SELECT va.*, u.UserName AS UserName " +
                        "FROM verlofaanvragen va " +
                        "INNER JOIN aspnetusers u ON va.UserId = u.Id";


                    using (var cmd = new MySqlCommand(query, connection))  
                    using (var adapter = new MySqlDataAdapter(cmd)) 
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                return dataTable;
            }

            return dataTable;
        }

    }
}
