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

                string connectionString = "Server=localhost;Port=3306;Database=GeoProfs;Uid=root;";

                string backupFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, backupFileName);

                string mysqldumpPath = @"C:\xampp\mysql\bin\mysqldump.exe"; 

                using (var process = new Process())
                {
                    process.StartInfo.FileName = mysqldumpPath; 
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
                // Log the error to the console
                Console.WriteLine($"An error occurred during the backup process: {ex.ToString()}");

                var errorModel = new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    ErrorMessage = ex.Message,
                };

                return View("Error", errorModel);   
            }
        }
    }
}
