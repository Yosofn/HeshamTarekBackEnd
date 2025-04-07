using DataAccessLayer.Data;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FreeFilesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FreeFilesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/FreeFiles
        [HttpGet("GetFreeFiles")]
        public async Task<ActionResult<IEnumerable<FreeFile>>> GetFreeFiles()
        {
            return await _context.FreeFiles.ToListAsync();
        }

        // POST: api/FreeFiles
        [HttpPost("AddFreeFile")]
        public async Task<ActionResult<FreeFile>> AddFreeFile(FreeFile freeFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.FreeFiles.Add(freeFile);
            await _context.SaveChangesAsync();

            return Ok(freeFile);
        }
    }
}