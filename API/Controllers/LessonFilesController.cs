using API.DTOs;
using DataAccessLayer.Data;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonFilesController : ControllerBase


    { 
        private readonly AppDbContext _context;

        public LessonFilesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/LessonFiles
        [HttpGet("GetAllLessonFiles")]
        public async Task<ActionResult<IEnumerable<LessonFile>>> GetAllLessonFiles()
        {
            var lessonFiles = await _context.LessonFiles.ToListAsync();

            return Ok(lessonFiles);
        }

        [HttpGet("GetLessonFilesByLessonId/{lessonId}")]
        public async Task<ActionResult<IEnumerable<LessonFile>>> GetLessonFilesByLessonId(int lessonId)
        {
            var lessonExists = await _context.Lessons.AnyAsync(l => l.Id == lessonId);
            if (!lessonExists)
            {
                return NotFound($"Lesson with ID = {lessonId} does not exist.");
            }

            var lessonFiles = await _context.LessonFiles
                                            .Where(lf => lf.LessonId == lessonId)
                                            .ToListAsync();

            if (!lessonFiles.Any())
            {
                return NotFound($"No lesson files found for LessonId = {lessonId}.");
            }

            return Ok(lessonFiles);
        }

        // POST: api/LessonFiles
        [HttpPost("AddLessonFile")]
        public async Task<ActionResult<LessonFileDto>> AddLessonFile(LessonFileDto createLessonFileDto)
        {

            var lessonExists = await _context.Lessons.AnyAsync(l => l.Id == createLessonFileDto.LessonId);
            if (!lessonExists)
            {
                return NotFound($"Lecture with ID = {createLessonFileDto.LessonId} does not exist.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var lessonFile = new LessonFile
            {
                LessonId = createLessonFileDto.LessonId,
                Title = createLessonFileDto.Title,
                Description = createLessonFileDto.Description,
                Url = createLessonFileDto.Url
            };

            _context.LessonFiles.Add(lessonFile);
            await _context.SaveChangesAsync();

         

            return Ok(lessonFile);
        }
    }
}

