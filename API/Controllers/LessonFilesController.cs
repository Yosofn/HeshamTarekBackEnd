using API.DTOs;
using DataAccessLayer.Data;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonFilesController : ControllerBase


    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;


        public LessonFilesController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;

        }


        [Authorize(Policy = "UserType")]

        [HttpPost("AddLessonFile")]
        public async Task<IActionResult> AddLessonFile(IFormFile file, int lessonId, string fileName, string? description, string lessonName)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                if (string.IsNullOrEmpty(lessonName))
                {
                    return NotFound("Lesson not found.");
                }

                var folderPath = Path.Combine(_env.WebRootPath, "LessonFiles", lessonName);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var lessonFile = new LessonFile
                {
                    LessonId = lessonId,
                    Title = fileName,
                    Description = description,
                };
                await _context.LessonFiles.AddAsync(lessonFile);
                await _context.SaveChangesAsync();

                return Ok("File uploaded and saved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [Authorize(Policy = "UserType")]

        [HttpPut("UpdateLessonFile/{id}")]
        public async Task<IActionResult> UpdateLessonFile(int id, IFormFile? newFile, string? newFileName, string? newDescription)
        {
            try
            {
                var lessonFile = await _context.LessonFiles.FindAsync(id);
                if (lessonFile == null)
                {
                    return NotFound("Lesson file not found.");
                }

                var lessonName = _context.Lessons
                    .Where(l => l.Id == lessonFile.LessonId)
                    .Select(l => l.Name)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(lessonName))
                {
                    return NotFound("Lesson not found.");
                }

                var lessonFolderPath = Path.Combine(_env.WebRootPath, "LessonFiles", lessonName);

                if (newFile != null && newFile.Length > 0)
                {
                    var oldFilePath = Path.Combine(lessonFolderPath, lessonFile.Title);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }

                    var newFilePath = Path.Combine(lessonFolderPath, newFileName);
                    using (var stream = new FileStream(newFilePath, FileMode.Create))
                    {
                        await newFile.CopyToAsync(stream); 
                    }

                    lessonFile.Title = newFileName;
                }

                if (!string.IsNullOrEmpty(newDescription))
                {
                    lessonFile.Description = newDescription;
                }

                _context.LessonFiles.Update(lessonFile);
                await _context.SaveChangesAsync();

                return Ok("Lesson file updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetFilesByLessonId/{lessonId}")]
        public async Task<IActionResult> GetFilesByLessonId(int lessonId)
        {
            try
            {
                // Step 1: Validate LessonId and fetch associated files
                var lessonFiles = await _context.LessonFiles
                    .Where(lf => lf.LessonId == lessonId)
                    .ToListAsync();
                var lessonName = _context.Lessons
               .Where(lf => lf.Id == lessonId)
               .Select(lf => lf.Name)
               .FirstOrDefault();

                if (lessonFiles == null || lessonFiles.Count == 0)
                {
                    return NotFound("No files found for this LessonId.");
                }

                // Step 2: Build the response with file information
                var filesResponse = lessonFiles.Select(file => new
                {
                    FileId = file.FileId,
                    FileName = file.Title,
                    Description = file.Description,
                    FileUrl = $"{Request.Scheme}://{Request.Host}/LessonFiles/{lessonName}/{file.Title}"
                });

                return Ok(filesResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{fileName}")]
        public IActionResult GetLessonFile(int lessonId, string fileName)
        {
            // Retrieve the lesson name
            var lessonName = _context.Lessons
                .Where(lf => lf.Id == lessonId)
                .Select(lf => lf.Name)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(lessonName))
            {
                return NotFound("Lesson not found.");
            }

            // Construct the file path
            var lessonFolderPath = Path.Combine(_env.WebRootPath, "LessonFiles", lessonName);
            var filePath = Path.Combine(lessonFolderPath, fileName);

            // Check if the file exists
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found.");
            }

            // Determine the content type
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType))
            {
                contentType = "application/octet-stream"; // Fallback for unknown file types
            }

            // Open and return the file
            var fileStream = System.IO.File.OpenRead(filePath);
            return File(fileStream, contentType, fileName);
        }

        [Authorize(Policy = "UserType")]

        [HttpDelete("DeleteLessonFile/{id}")]
        public async Task<IActionResult> DeleteLessonFile(int id)
        {
            try
            {
                var lessonFile = await _context.LessonFiles.FindAsync(id);
                if (lessonFile == null)
                {
                    return NotFound("Lesson file not found.");
                }

                var lessonName = _context.Lessons
                    .Where(l => l.Id == lessonFile.LessonId)
                    .Select(l => l.Name)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(lessonName))
                {
                    return NotFound("Lesson not found.");
                }

                var lessonFolderPath = Path.Combine(_env.WebRootPath, "LessonFiles", lessonName);
                var filePath = Path.Combine(lessonFolderPath, lessonFile.Title);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.LessonFiles.Remove(lessonFile);
                await _context.SaveChangesAsync();

                return Ok("Lesson file deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }



}

