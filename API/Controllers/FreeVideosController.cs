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
    public class FreeVideosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FreeVideosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/FreeVideos
        [HttpGet("GetFreeVideos")]
        public async Task<ActionResult<IEnumerable<FreeVideo>>> GetFreeVideos()
        {
            return await _context.FreeVideos.ToListAsync();
        }
        [HttpGet("SearchFreeVideos")]
        public async Task<IActionResult> SearchFreeVideos([FromQuery] string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                var  freeVideos = await _context.FreeVideos
                    
                    .ToListAsync();
                return Ok(freeVideos);

            }
            else
            {
                var freeVideos = await _context.FreeVideos
                    .Where(v => (v.Title != null && v.Title.ToLower().Contains(query.ToLower())) ||
                                (v.Description != null && v.Description.ToLower().Contains(query.ToLower())))
                    .ToListAsync();

                if (!freeVideos.Any())
                {
                    return NotFound("No videos found matching the search query.");
                }

                return Ok(freeVideos);
            }
            }

        [Authorize(Policy = "UserType")]
        [HttpPost("AddFreeVideo")]
        public async Task<ActionResult<FreeVideo>> AddFreeVideo(FreeVideo freeVideo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.FreeVideos.Add(freeVideo);
            await _context.SaveChangesAsync();

            return Ok(freeVideo);
        }

        [Authorize(Policy = "UserType")]

        [HttpPut("UpdateFreeVideo/{id}")]
        public async Task<IActionResult> UpdateFreeVideo(int id, [FromBody] FreeVideo freeVideo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingVideo = await _context.FreeVideos.FindAsync(id);
            if (existingVideo == null)
            {
                return NotFound("FreeVideo not found.");
            }

            existingVideo.Title = freeVideo.Title;
            existingVideo.Description = freeVideo.Description;
            existingVideo.Url = freeVideo.Url;

            _context.FreeVideos.Update(existingVideo);
            await _context.SaveChangesAsync();

            return Ok(existingVideo);
        }

        [Authorize(Policy = "UserType")]

        [HttpDelete("DeleteFreeVideo/{id}")]
        public async Task<IActionResult> DeleteFreeVideo(int id)
        {
            var freeVideo = await _context.FreeVideos.FindAsync(id);
            if (freeVideo == null)
            {
                return NotFound("FreeVideo not found.");
            }

            _context.FreeVideos.Remove(freeVideo);
            await _context.SaveChangesAsync();

            return Ok($"FreeVideo with ID {id} has been deleted successfully.");
        }
    }
}
