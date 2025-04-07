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
    public class LectureFilesController : ControllerBase
    {
        
            private readonly AppDbContext _context;

            public LectureFilesController(AppDbContext context)
            {
                _context = context;
            }

            // GET: api/LectureFiles
            [HttpGet("GetLectureFiles")]
            public async Task<ActionResult<IEnumerable<LectureFile>>> GetLectureFiles()
            {
                var lectureFiles = await _context.LectureFiles.ToListAsync();

                return Ok(lectureFiles);
            }
        [HttpGet("GetLectureFilesByLectureId/{lectureId}")]
        public async Task<ActionResult<IEnumerable<LectureFile>>> GetLectureFilesByLectureId(int lectureId)
        {
            // التحقق من أن المحاضرة موجودة في قاعدة البيانات
            var lectureExists = await _context.Lectures.AnyAsync(l => l.Id == lectureId);
            if (!lectureExists)
            {
                return NotFound($"Lecture with ID = {lectureId} does not exist.");
            }

            // البحث عن الملفات المرتبطة بـ LectureId
            var lectureFiles = await _context.LectureFiles
                                             .Where(lf => lf.LectureId == lectureId)
                                             .ToListAsync();

            // إذا لم يتم العثور على أي ملفات، إرجاع استجابة NotFound
            if (!lectureFiles.Any())
            {
                return NotFound($"No lecture files found for LectureId = {lectureId}.");
            }

            // إذا تم العثور على ملفات، إرجاعها كاستجابة
            return Ok(lectureFiles);
        }
        // POST: api/LectureFiles
        [HttpPost("AddLectureFile")]
            public async Task<ActionResult<LectureFile>> AddLectureFile(LectureFileDto createLectureFileDto)
            {

            var lectureExists = await _context.Lectures.AnyAsync(l => l.Id == createLectureFileDto.LectureId);
            if (!lectureExists)
            {
                return NotFound($"Lecture with ID = {createLectureFileDto.LectureId} does not exist.");
            }
            if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // الخرائط اليدوية من DTO إلى Model
                var lectureFile = new LectureFile
                {
                    LectureId = createLectureFileDto.LectureId,
                    Title = createLectureFileDto.Title,
                    Description = createLectureFileDto.Description,
                    Url = createLectureFileDto.Url
                };

                _context.LectureFiles.Add(lectureFile);
                await _context.SaveChangesAsync();

            

                return Ok(lectureFile);
            }
        }
    }

