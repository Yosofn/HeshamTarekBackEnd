using API.DTOs;
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
    public class LessonsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public LessonsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        [Authorize(Policy = "UserType")]
        [HttpGet("GetAdminPaginatedLessons")]
        public async Task<IActionResult> GetAdminPaginatedLessons(int pageNumber, int pageSize)
        {
            try
            {
                // Calculate the skip value based on page number and page size
                var skip = (pageNumber - 1) * pageSize;

                // Query the View using raw SQL
                var lessons = await _context.LessonDetails // Replace with your DbContext View name
                    .FromSqlInterpolated($"SELECT * FROM vw_LessonDetails ORDER BY LessonId OFFSET {skip} ROWS FETCH NEXT {pageSize} ROWS ONLY")
                    .ToListAsync();

                // Get the total number of lessons for pagination metadata
                var totalLessons = await _context.LessonDetails.CountAsync();

                // Calculate the total number of pages
                var totalPages = (int)Math.Ceiling((double)totalLessons / pageSize);

                // Create pagination metadata
                var paginationMetadata = new PaginationMetadata
                {
                    TotalLessons = totalLessons,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages
                };

                // Return the paginated response with the metadata
                var response = new PaginatedLessonsResponse
                {
                    Lessons = lessons,
                    PaginationMetadata = paginationMetadata
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("GetAdminAllLessons")]
        public async Task<IActionResult> GetAdminAllLessons()
        {
            var lessons = await _context.Lessons
                .Select(l => new
                {
                    l.Id,
                    l.Name,
                })
                .ToListAsync();

            return Ok(lessons);
        }

        [Authorize(Policy = "UserType")]
        [HttpDelete("DeleteLesson")]
        public async Task<IActionResult> DeleteLesson(int userId, int lessonId)
        {
            // تحقق من وجود السجل في جدول BlockedLectures
            var blockedLecture = await _context.BlockedLectures
                .FirstOrDefaultAsync(bl => bl.UserId == userId && bl.LessonId == lessonId);

            if (blockedLecture == null)
            {
                return NotFound($"Lessons with UserId {userId} and LessonId {lessonId} not found.");
            }

            // حذف السجل من جدول BlockedLectures
            _context.BlockedLectures.Remove(blockedLecture);
            await _context.SaveChangesAsync();

            return Ok("Lessons deleted successfully.");
        }

        [HttpGet("GetPaginatedLessons")]
        public async Task<IActionResult> GetPaginatedLessons(int pageNumber, int pageSize)
        {
            try
            {
                // Calculate the skip value based on page number and page size
                var skip = (pageNumber - 1) * pageSize;

                // Query the View using raw SQL
                var lessons = await _context.LessonDetailsWithoutEncryption // Replace with your DbContext View name
                    .FromSqlInterpolated($"SELECT * FROM vw_LessonDetailsWithoutEncryption ORDER BY LessonId OFFSET {skip} ROWS FETCH NEXT {pageSize} ROWS ONLY")
                    .ToListAsync();

                // Get the total number of lessons for pagination metadata
                var totalLessons = await _context.LessonDetailsWithoutEncryption.CountAsync();

                // Calculate the total number of pages
                var totalPages = (int)Math.Ceiling((double)totalLessons / pageSize);

                // Create pagination metadata
                var paginationMetadata = new PaginationMetadata
                {
                    TotalLessons = totalLessons,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages
                };

                var response = new PaginatedLessonsWithoutEncryptionResponse
                {
                    Lessons = lessons,
                    PaginationMetadata = paginationMetadata
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [Authorize]
        [HttpGet("GetLessonsByUserId/{userId}")]
        public async Task<IActionResult> GetLessonsByUserId(int userId)
        {
            var lessonIds = await _context.BlockedLectures
                .Where(bl => bl.UserId == userId)
                .Select(bl => bl.LessonId)
                .ToListAsync();

            if (!lessonIds.Any())
            {
                return NotFound(new { message = "No blocked lessons found for this user." });
            }

            var lessonDetails = new List<LessonDetails>();
            var allLessons = _context.LessonDetails.AsEnumerable();

            foreach (var lessonId in lessonIds)
            {
                var lesson = allLessons.FirstOrDefault(ld => ld.LessonId == lessonId);
                if (lesson != null)
                {
                    lessonDetails.Add(lesson);
                }
            }



            // Step 3: Return the lesson details as the response
            return Ok(lessonDetails);
        }

        [HttpGet("GetAllLessonsWithUserStatus/{userId}")]

        public async Task<IActionResult> GetAllLessonsWithUserStatus(int userId)
        {

            // Step 1: Get all LessonIds from BlockedLectures for the specified UserId
            var blockedLessonIds = await _context.BlockedLectures
                .Where(bl => bl.UserId == userId)
                .Select(bl => bl.LessonId)
                .ToListAsync();

            // Step 2: Get all lessons from the Lessons table
            var allLessons = await _context.Lessons.ToListAsync();

            // Step 3: Add IsBought property to each lesson
            var lessonsWithStatus = allLessons.Select(lesson => new
            {
                lesson.Id,
                lesson.Name,
                lesson.Price,
                lesson.Description,
                IsBought = blockedLessonIds.Contains(lesson.Id)
            }).ToList();

            // Step 4: Return the lessons with status
            return Ok(lessonsWithStatus);
        }

        [HttpGet("GetLessonByLessonId{lessonId}")]
        public async Task<IActionResult> GetLessonByLessonId(int lessonId)
        {

            var lesson = await _context.Lessons
                .Where(l => l.Id == lessonId)
                .FirstOrDefaultAsync();

            if (lesson == null)
            {
                return NotFound();
            }

            // Step 3: Add IsBought property to the lesson
            var lessonWithStatus = new
            {
                lesson.Id,
                lesson.Name,
                lesson.Price,
                lesson.Description,
            };

            // Step 4: Return the lesson with status
            return Ok(lessonWithStatus);
        }



        [Authorize(Policy = "UserType")]
        [HttpPost("CreateLesson")]
        public async Task<IActionResult> CreateLesson([FromForm] CreateUpdateLessonDTO courseDto)
        {
            if (courseDto.Image == null || courseDto.Image.Length == 0)
                return BadRequest("No image file provided.");

            var maxLessonPriority = _context.Lessons.Max(l => (int?)l.LessonPriority) ?? 0;
            var newLessonPriority = maxLessonPriority + 1;

            // إنشاء الكورس وحفظه أولاً للحصول على ID
            var course = new Lesson
            {
                Name = courseDto.Name,
                Description = courseDto.Description,
                Price = courseDto.Price,
                Date = DateTime.Now,
                LessonPriority = newLessonPriority,
                EncryptionCode = courseDto.EncryptionCode,
                LessonState = courseDto.LessonState,
                TeacherId = courseDto.TeacherId,
                MaterialId = courseDto.MaterialId,

            };

            _context.Lessons.Add(course);
            await _context.SaveChangesAsync();

            // استخدام ID الكورس لحفظ الصورة

            var imageFileName = $"{course.Id}.jpg";
            var filePath = Path.Combine(_env.WebRootPath, "CourseImages", imageFileName);
            if (!Directory.Exists(Path.Combine(_env.WebRootPath, "CourseImages")))
            {
                Directory.CreateDirectory(Path.Combine(_env.WebRootPath, "CourseImages"));
            }
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await courseDto.Image.CopyToAsync(stream);
            }



            return Ok(course);
        }


        [Authorize(Policy = "UserType")]

        [HttpPut("UpdateLesson/{id}")]
        public async Task<IActionResult> UpdateLesson(int id, [FromForm] CreateUpdateLessonDTO courseDto)
        {
            var course = await _context.Lessons.FindAsync(id);
            if (course == null)
            {
                return NotFound("Lesson not found");
            }

            course.Name = courseDto.Name;
            course.Description = courseDto.Description;
            course.Price = courseDto.Price;
            course.Date = DateTime.Now;
            course.LessonPriority = courseDto.LessonPriority;
            course.EncryptionCode = courseDto.EncryptionCode;
            course.LessonState = courseDto.LessonState;
            course.TeacherId = courseDto.TeacherId;
            course.MaterialId = courseDto.MaterialId;

            if (courseDto.Image != null && courseDto.Image.Length > 0)
            {
                var imageFileName = $"{course.Id}.jpg";
                var filePath = Path.Combine(_env.WebRootPath, "CourseImages", imageFileName);
                if (!Directory.Exists(Path.Combine(_env.WebRootPath, "CourseImages")))
                {
                    Directory.CreateDirectory(Path.Combine(_env.WebRootPath, "CourseImages"));
                }
                if (Directory.Exists(filePath))
                {
                    Directory.Delete(filePath);
                }
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await courseDto.Image.CopyToAsync(stream);
                }
            }

            _context.Lessons.Update(course);
            await _context.SaveChangesAsync();

            return Ok(course);
        }

        [HttpGet("{id}")]
        public IActionResult GetImage(int id)
        {
            var imagePath = Path.Combine(_env.WebRootPath, "CourseImages", $"{id}.jpg");
            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound("Image not found");
            }

            var imageFileStream = System.IO.File.OpenRead(imagePath);
            return File(imageFileStream, "image/jpeg");
        }
    }

    public class PaginationMetadata
    {
        public int TotalLessons { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class PaginatedLessonsResponse
    {
        public List<LessonDetails>? Lessons { get; set; }
        public PaginationMetadata? PaginationMetadata { get; set; }
    }
    public class PaginatedLessonsWithoutEncryptionResponse
    {
        public List<LessonDetailsWithoutEncryption>? Lessons { get; set; }
        public PaginationMetadata? PaginationMetadata { get; set; }
    }



    }
