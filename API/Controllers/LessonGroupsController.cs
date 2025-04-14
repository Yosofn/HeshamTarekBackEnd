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
    public class LessonGroupsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LessonGroupsController(AppDbContext context)
        {
            _context = context;
        }
        [Authorize]

        [HttpGet("GetLessonGroups")]
        public async Task<IActionResult> GetLessonGroups()
        {
            var lessonGroups = await _context.LessonGroups.ToListAsync();
            return Ok(lessonGroups);
        }
        [Authorize]


        [HttpGet("GetLessonGroupsByLessonId/{lessonId}")]
        public async Task<IActionResult> GetLessonGroupsByLessonId(int lessonId)
        {
            var lessonGroups = await _context.LessonGroups
                .Where(lg => lg.LessonId == lessonId)
                .ToListAsync();

            if (lessonGroups == null || lessonGroups.Count == 0)
            {
                return NotFound($"No LessonGroups found for LessonId {lessonId}.");
            }

            return Ok(lessonGroups);
        }
        [Authorize(Policy = "UserType")]

        [HttpPost("AddLessonGroup")]
        public async Task<IActionResult> AddLessonGroup([FromBody] LessonGroupDto lessonGroupDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var lessonExists = await _context.Lessons.AnyAsync(l => l.Id == lessonGroupDto.LessonId);
            if (!lessonExists)
            {
                return BadRequest("Invalid LessonId: Lesson does not exist.");
            }

            var lessonGroup = new LessonGroups
            {
                Name = lessonGroupDto.Name,
                Link = lessonGroupDto.Link,
                LessonId = lessonGroupDto.LessonId,
                Type = lessonGroupDto.Type
            };

            _context.LessonGroups.Add(lessonGroup);
            await _context.SaveChangesAsync();
            return Ok(lessonGroup);
        }

        [Authorize(Policy = "UserType")]
        [HttpPut("EditLessonGroup{id}")]
        public async Task<IActionResult> EditLessonGroup(int id, [FromBody] LessonGroupDto lessonGroupDto)
        {
            var lessonGroup = await _context.LessonGroups.FindAsync(id);
            if (lessonGroup == null)
            {
                return NotFound("LessonGroup not found.");
            }

            // تحقق من صحة LessonId قبل التحديث
            var lessonExists = await _context.Lessons.AnyAsync(l => l.Id == lessonGroupDto.LessonId);
            if (!lessonExists)
            {
                return BadRequest("Invalid LessonId: Lesson does not exist.");
            }

            lessonGroup.Name = lessonGroupDto.Name;
            lessonGroup.Link = lessonGroupDto.Link;
            lessonGroup.LessonId = lessonGroupDto.LessonId; // Assign after validation
            lessonGroup.Type = lessonGroupDto.Type;

            _context.LessonGroups.Update(lessonGroup);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [Authorize(Policy = "UserType")]

        [HttpDelete("DeleteLessonGroup{id}")]
        public async Task<IActionResult> DeleteLessonGroup(int id)
        {
            var lessonGroup = await _context.LessonGroups.FindAsync(id);
            if (lessonGroup == null)
            {
                return NotFound("LessonGroup not found.");
            }

            _context.LessonGroups.Remove(lessonGroup);
            await _context.SaveChangesAsync();
            return Ok("LessonGroup deleted successfully.");
        }

    }
}
