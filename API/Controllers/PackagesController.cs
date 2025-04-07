using API.DTOs;
using DataAccessLayer.Data;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using static API.Controllers.PackagesController;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackagesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PackagesController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        [Authorize(Policy = "UserType")]

        [HttpPost("AddPackage")]
        public async Task<IActionResult> AddPackage([FromForm] PackageWithLessonsDto request)
        {
            //if (request == null || string.IsNullOrEmpty(request.Title) )
            //{
            //    return BadRequest("Invalid input data");
            //}

            var newPackage = new Package
            {
                Name = request.Title,
                Description = request.Description,
                Price = request.Price,
                OriginalPrice = request.OriginalPrice
            };

            _context.Packages.Add(newPackage);
            await _context.SaveChangesAsync();
            var packageId = newPackage.PackageId;

            if (request.Image != null && request.Image.Length > 0)
            {
                var imagePath = Path.Combine(_env.WebRootPath, "Packages", $"{packageId}.jpg");
                Directory.CreateDirectory(Path.GetDirectoryName(imagePath)!);

                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await request.Image.CopyToAsync(stream);
                }
            }
            if (request.LessonIds!=null)
            {
                var packageLessons = request.LessonIds.Select(lessonId => new PackageLesson
                {
                    PackageId = packageId,
                    LessonId = lessonId
                });

                _context.PackageLessons.AddRange(packageLessons);
                await _context.SaveChangesAsync();
            }
            return Ok(new { Message = "Package added successfully", package = newPackage });
        }

        [Authorize(Policy = "UserType")]

        [HttpPut("UpdatePackage/{id}")]
        public async Task<IActionResult> UpdatePackage(int id, [FromForm] PackageWithLessonsDto request)
        {
            var existingPackage = await _context.Packages.FindAsync(id);
            if (existingPackage == null)
            {
                return NotFound(new { Message = "Package not found" });
            }

            existingPackage.Name = request.Title ?? existingPackage.Name; 
            existingPackage.Description = request.Description ?? existingPackage.Description;
            existingPackage.Price = request.Price ?? existingPackage.Price;
            existingPackage.OriginalPrice = request.OriginalPrice ?? existingPackage.OriginalPrice;

            // Step 3: Update the image if provided
            if (request.Image != null && request.Image.Length > 0)
            {
                var imagePath = Path.Combine(_env.WebRootPath, "Packages", $"{existingPackage.PackageId}.jpg");
                Directory.CreateDirectory(Path.GetDirectoryName(imagePath)!); // Ensure directory exists
                if (Directory.Exists(imagePath))
                {
                    Directory.Delete(imagePath);
                }
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await request.Image.CopyToAsync(stream);
                }
            }

            if (request.LessonIds != null && request.LessonIds.Any())
            {
                var existingLessons = _context.PackageLessons.Where(pl => pl.PackageId == id);
                _context.PackageLessons.RemoveRange(existingLessons);

                var packageLessons = request.LessonIds.Select(lessonId => new PackageLesson
                {
                    PackageId = id,
                    LessonId = lessonId
                });
                _context.PackageLessons.AddRange(packageLessons);
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Package updated successfully", Package = existingPackage });
        }

        [HttpGet("GetPackagesWithLessons")]
        public async Task<IActionResult> GetPackagesWithLessons(string query = null)
        {
            try
            {
                var queryable = _context.Packages
                    .Include(p => p.PackageLessons)
                    .ThenInclude(pl => pl.Lesson) 
                    .AsQueryable();

                if (!string.IsNullOrEmpty(query))
                {
                    queryable = queryable.Where(p =>
                        p.Name.Contains(query) || p.Description.Contains(query));
                }

                var packagesWithLessons = await queryable
                    
                    .ToListAsync();
                

                var result = packagesWithLessons.Select(p => new
                {
                    PackageId = p.PackageId,
                    Title = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    OriginalPrice = p.OriginalPrice,
                    TotalNumberOfStudents = p.PackageLessons
                    .Select(pl => pl.Lesson.Id) // Collect all lesson IDs
                    .Distinct()
                    .SelectMany<int, int>(lessonId => _context.BlockedLectures
                    .Where(bl => bl.LessonId == lessonId) // Match lessons in BlockedLectures
                   .Select(bl => bl.UserId)
                   .Distinct())
                  .Distinct()
                   .Count(),
                    TotalNumberOfLectures = p.PackageLessons
                    .Select(pl => pl.Lesson.Id) // Collect all lesson IDs
                    .Distinct()
                    .SelectMany(lessonId => _context.Lectures
                    .Where(l => l.LessonId == lessonId) // Match lectures in the course
                    .Select(l => l.Id)) // Get lecture IDs
                    .Count(),
                    Lessons = p.PackageLessons
                        .Select(pl => pl.Lesson) 
                        .Where(lesson => lesson != null) 
                        .Select(lesson => new
                        {
                            Id = lesson.Id,
                            Name = lesson.Name,
                            Description = lesson.Description,
                            Teacher=lesson.TeacherId,
                            Price=lesson.Price,
                            State=lesson.LessonState
                        })
                        .ToList()
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("GetPackages")]
        public async Task<List<Package>> GetPackages()
        {
            var packagesWithLessons = await _context.Packages
                .Include(p => p.PackageLessons)
                // هذا إذا كان هناك كلاس Lesson مرتبط بـ PackageLesson
                .ToListAsync();

            return packagesWithLessons;
        }

        [HttpGet("GetPackageDetails/{packageId}")]
        public async Task<IActionResult> GetPackageDetails(int packageId)
        {
            try
            {
                // Step 1: Retrieve the package by ID
                var package = await _context.Packages
                    .Include(p => p.PackageLessons)
                    .ThenInclude(pl => pl.Lesson) // Include lessons
                    .FirstOrDefaultAsync(p => p.PackageId == packageId);

                // Step 2: Handle the case when the package is not found
                if (package == null)
                {
                    return NotFound(new { Message = "Package not found" });
                }

                // Step 3: Build the response object
                var result = new
                {
                    PackageId = package.PackageId,
                    Title = package.Name,
                    Description = package.Description,
                    Price = package.Price,
                    OriginalPrice = package.OriginalPrice,
                    Lessons = package.PackageLessons.Select(pl => new
                    {
                        CourseId = pl.Lesson.Id,
                        Name = pl.Lesson.Name,
                        Description = pl.Lesson.Description,
                        TeacherName = _context.Users
                            .Where(u => u.Id == pl.Lesson.TeacherId) // Match TeacherId
                            .Select(u => u.Name) // Fetch teacher name
                            .FirstOrDefault(),
                        Price = pl.Lesson.Price,
                        State = pl.Lesson.LessonState,
                         TotalLectures = _context.Lectures
                    .Where(l => l.LessonId == pl.Lesson.Id) // Match lectures for this course
                    .Count(),

                        // Total Students for the Course
                        TotalStudents = _context.BlockedLectures
                    .Where(bl => bl.LessonId == pl.Lesson.Id) // Match students for this course
                    .Select(bl => bl.UserId) // Get student IDs
                    .Distinct() // Avoid duplicates
                    .Count()
                    }).ToList()
                };

                // Step 4: Return the result
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Return an error response in case of failure
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("{id}")]
        public IActionResult GetImage(int id)
        {
            var imagePath = Path.Combine(_env.WebRootPath, "Packages", $"{id}.jpg");
            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound("Image not found");
            }

            var imageFileStream = System.IO.File.OpenRead(imagePath);
            return File(imageFileStream, "image/jpeg");
        }
        [Authorize(Policy = "UserType")]

        [HttpDelete("DeletePackageLesson")]
        public async Task<IActionResult> DeletePackageLesson(int packageId, int lessonId)
        {
            try
            {
                var packageLesson = await _context.PackageLessons
                    .FirstOrDefaultAsync(pl => pl.PackageId == packageId && pl.LessonId == lessonId);

                if (packageLesson == null)
                {
                    return NotFound(new { Message = "PackageLesson not found" });
                }

                _context.PackageLessons.Remove(packageLesson);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "PackageLesson deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize(Policy = "UserType")]

        [HttpDelete("DeletePackage/{id}")]
        public async Task<IActionResult> DeletePackage(int id)
        {
            var package = await _context.Packages
                .Include(p => p.PackageLessons) 
                .FirstOrDefaultAsync(p => p.PackageId == id);

            if (package == null)
            {
                return NotFound(new { Message = "Package not found" });
            }

            _context.Packages.Remove(package);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Package and related lessons deleted successfully" });
        }
    }

    public class PackageDto
        {
            public int PackageId { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public string? FilePath { get; set; }
            public decimal? Price { get; set; }

            public decimal? OriginalPrice { get; set; }
            public int NumberOfLectures { get; set; }
            public int NumberOfStudents { get; set; }
        }

    }

