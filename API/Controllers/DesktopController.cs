using DataAccessLayer.Data;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DesktopController : ControllerBase
    {
        private readonly AppDbContext _context;

        private readonly string _rootDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        private readonly string _desktopDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Desktop");
        public DesktopController(AppDbContext context)
        {
            _context = context;

        }
        [HttpPost("CreateVersion")]
        public async Task<IActionResult> CreateVersion([FromBody] string version)
        {
            var desktop = new Desktop
            {

                Version = version
            };
            _context.Desktop.Add(desktop);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Version created successfully" });
        }

        // 2. نقطة النهاية للحصول على جميع النسخ
        [HttpGet("GetAllVersions")]
        public async Task<IActionResult> GetAllVersions()
        {
            var versions = await _context.Desktop.ToListAsync();
            return Ok(versions);
        }

        // 3. نقطة النهاية للحصول على أحدث نسخة
        [HttpGet("GetLatestVersion")]
        public async Task<IActionResult> GetLatestVersion()
        {
            var latestVersion = await _context.Desktop
                .OrderByDescending(d => d.Id)
                .FirstOrDefaultAsync();

            if (latestVersion == null)
            {
                return NotFound("No versions found");
            }

            return Ok(latestVersion);
        }

        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided");
            }
            if (!Directory.Exists(_desktopDirectory))
            {
                Directory.CreateDirectory(_desktopDirectory);
            }
            var files = Directory.GetFiles(_desktopDirectory);
            foreach (var f in files)
            {
                System.IO.File.Delete(f);
            }
            var filePath = Path.Combine(_desktopDirectory, file.FileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Ok(new { Message = "File uploaded and replaced successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetFile/{fileName}")]
        public IActionResult GetFile(string fileName)
        {
            // مسار الملف في المجلد Desktop
            var filePath = Path.Combine(_desktopDirectory, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found");
            }

            // إرجاع الملف مباشرةً من نظام الملفات
            return PhysicalFile(filePath, "application/octet-stream", fileName);

        }


    }
}
