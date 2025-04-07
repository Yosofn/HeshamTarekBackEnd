using API.DTOs;
using DataAccessLayer.Data;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LecturesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public LecturesController(AppDbContext context)
        {
            _context = context;
        }
        [Authorize]
        [HttpGet("GetAdminLectures")]
        public async Task<IActionResult> GetAdminLectures(int lessonId, int pageNumber, int pageSize, string query = null)
        {
            // Validate pagination parameters
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest("Page number and page size must be greater than 0.");
            }

            try
            {
                bool isPriority = int.TryParse(query, out int priority);
                var queryable = _context.Lectures.Where(l => l.LessonId == lessonId).AsQueryable();

                if (!string.IsNullOrEmpty(query))
                {
                    if (isPriority)
                    {
                        queryable = queryable.Where(l => l.Priority == priority);
                    }
                    else
                    {
                        queryable = queryable.Where(l => l.Name.Contains(query));
                    }
                }

                // Define pagination parameters
                var paginatedLectures = await queryable
                    .OrderBy(l=>l.Priority)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var totalLectures = await queryable.CountAsync();
                // Calculate total pages
                var totalPages = (int)Math.Ceiling(totalLectures / (double)pageSize);

                // Prepare pagination metadata
                var paginationMetadata = new
                {
                    totalLectures,
                    pageNumber,
                    pageSize,
                    totalPages
                };

                // Return the lectures and pagination metadata
                return Ok(new { paginatedLectures, paginationMetadata });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        
        [HttpGet("GetLectures")]

        public async Task<IActionResult> GetLectures(int lessonId, int pageNumber, int pageSize)
        { // Validate pagination parameters
          if (pageNumber < 1 || pageSize < 1) 
            { return BadRequest("Page number and page size must be greater than 0.");
            } try { 
                // Define stored procedure parameters
                var parameters = new[] { new SqlParameter("@LessonId", lessonId), 
                    new SqlParameter("@PageNumber", pageNumber), new SqlParameter("@PageSize", pageSize) };

                var lecturesWithTotalCount = await _context.Lectures 
                    .FromSqlRaw("EXEC GetPaginatedLectures @LessonId, @PageNumber, @PageSize", parameters) 
                    .ToListAsync();
                var totalLectures = await _context.Lectures .Where(l => l.LessonId == lessonId) .CountAsync(); 
                // Calculate total pages
                var totalPages = (int)Math.Ceiling(totalLectures / (double)pageSize);
                // Prepare pagination metadata
                var paginationMetadata = new { totalLectures, pageNumber, pageSize, totalPages };
                // Return the lectures and pagination metadata'
                return Ok(new { lecturesWithTotalCount, paginationMetadata }); }
            catch (Exception ex) { return StatusCode(500, $"Internal server error: {ex.Message}"); } } 


            [Authorize(Policy = "UserType")]

        [HttpPost("AdminCreateLecture")]
        public async Task<IActionResult> AdminCreateLecture(int lessonId, [FromBody] CreateUpdateLectureDTO lectureDTO)
        {
            try
            {
                // جلب اسم المعلم من LessonId
                var lesson = await _context.Lessons.FindAsync(lessonId);
                if (lesson == null)
                {
                    return NotFound("Lesson not found");
                }

                   var existingLectures = await _context.Lectures
                  .Where(l => l.LessonId == lessonId && l.Priority >= lectureDTO.Priority)
                 .OrderBy(l => l.Priority)
                 .ToListAsync();

                // زيادة الأولوية للمحاضرات التي تأتي بعد الأولوية المحددة
                foreach (var existingLecture in existingLectures)
                {
                    existingLecture.Priority += 1;
                }

                // تحديث الأولويات في قاعدة البيانات
                _context.Lectures.UpdateRange(existingLectures);
                var lecture = new Lecture
                {
                    UserId = lesson.TeacherId,
                    MaterialId = lesson.MaterialId,
                    Name = lectureDTO.Name,
                    Date = DateTime.UtcNow,
                    Url = lectureDTO.Url,
                    Price = 0,
                    DependancyId = null,
                    DependancyType = null,
                    LessonId = lessonId,
                    Priority = lectureDTO.Priority,
                    LectureDescription = lectureDTO.LectureDescription,
                    Drive=lectureDTO.Drive
                };

                _context.Lectures.Add(lecture);
                await _context.SaveChangesAsync();

                // إنشاء المحاضرة

                // إرجاع استجابة النجاح
                return Ok(new { Message = "Lecture created successfully"});
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize(Policy = "UserType")]

        [HttpPut("AdminUpdateLecture/{id}")]
        public async Task<IActionResult> AdminUpdateLecture(int id, [FromBody] CreateUpdateLectureDTO lectureDTO)
        {
            try
            {
                var existingLecture = await _context.Lectures.FindAsync(id);
                if (existingLecture == null)
                {
                    return NotFound("Lecture not found");
                }

                var existingLectures = await _context.Lectures
                    .Where(l => l.LessonId == existingLecture.LessonId)
                    .OrderBy(l => l.Priority)
                    .ToListAsync();

                if (existingLectures.Any() && lectureDTO.Priority <= existingLectures.Last().Priority)
                {
                    foreach (var lecture in existingLectures)
                    {
                        if (lecture.Priority >= lectureDTO.Priority && lecture.Id != id)
                        {
                            lecture.Priority += 1;
                        }
                    }

                    _context.Lectures.UpdateRange(existingLectures);
                }

                existingLecture.Name = lectureDTO.Name;
                existingLecture.Url = lectureDTO.Url;
                existingLecture.Priority = lectureDTO.Priority;
                existingLecture.LectureDescription = lectureDTO.LectureDescription;
                existingLecture.Drive=lectureDTO.Drive;

                _context.Lectures.Update(existingLecture);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Lecture updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [Authorize]

        [HttpGet("SearchLectures")]
        public async Task<IActionResult> SearchLectures(int lessonId, string query)
        {
            try
            {
                // التحقق مما إذا كان الإدخال عددياً (بحث بواسطة الأولوية) أو نصياً (بحث بواسطة الاسم)
                bool isPriority = int.TryParse(query, out int priority);

                var queryable = _context.Lectures.Where(l => l.LessonId == lessonId).AsQueryable();

                // إضافة شرط لتصفية البحث بناءً على lessonId

                if (isPriority)
                {
                    queryable = queryable.Where(l => l.Priority == priority);
                }
                else
                {
                    queryable = queryable.Where(l => l.Name.Contains(query));
                }

                var result = await queryable.ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize(Policy = "UserType")]

        [HttpDelete("DeleteLecture/{lectureId}")]
        public async Task<IActionResult> DeleteLecture(int lectureId)
        {
            try
            {
                // البحث عن المحاضرة بواسطة LectureId
                var lecture = await _context.Lectures.FindAsync(lectureId);
                if (lecture == null)
                {
                    return NotFound("Lecture not found");
                }

                // إزالة المحاضرة من قاعدة البيانات
                _context.Lectures.Remove(lecture);
                await _context.SaveChangesAsync();

                // إرجاع استجابة النجاح
                return Ok(new { Message = "Lecture deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


    }
}