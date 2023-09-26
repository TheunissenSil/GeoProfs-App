using GeoProfs_App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;

namespace GeoProfs_App.Controllers
{
    [Authorize(Roles = "ICT")]
    public class IctController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Backup()
        {
            try
            {
                string backupFileName = "GeoProfs_Backup_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".sql";

                // Replace the following connection string with your MySQL database connection string.
                string connectionString = "Server=localhost;Port=3306;Database=GeoProfs;Uid=root;";

                // Path where the backup file will be saved.
                string backupFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, backupFileName);

                // Use mysqldump to create a database backup.
                using (var process = new Process())
                {
                    process.StartInfo.FileName = "mysqldump";
                    process.StartInfo.Arguments = $"--host=localhost --port=3306 --user=root GeoProfs --result-file=\"{backupFilePath}\"";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.Start();

                    process.WaitForExit();
                }

                // Return the backup file for download.
                var stream = new FileStream(backupFilePath, FileMode.Open);
                return File(stream, "application/octet-stream", backupFileName);
            }
            catch (Exception ex)
            {
                // Log the error (you can use a logging library like Serilog, NLog, or built-in logging)

                // Create an error message or object to pass to the error view
                var errorModel = new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    ErrorMessage = ex.Message, // Pass the error message to the view
                };

                return View("Error", errorModel);
            }
        }
    }
}
