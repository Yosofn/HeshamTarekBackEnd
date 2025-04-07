using DataAccessLayer.Data;
using DataAccessLayer.Models;
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

        // POST: api/FreeVideos
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
    }
}
