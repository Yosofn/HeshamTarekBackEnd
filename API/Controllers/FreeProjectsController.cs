using DataAccessLayer.Data;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FreeProjectsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FreeProjectsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/FreeProjects
        [HttpGet("GetFreeProjects")]
        public async Task<ActionResult<IEnumerable<FreeProject>>> GetFreeProjects()
        {
            return await _context.FreeProjects.ToListAsync();
        }

        // POST: api/FreeProjects
        [HttpPost("AddFreeProject")]
        public async Task<ActionResult<FreeProject>> AddFreeProject(FreeProject freeProject)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.FreeProjects.Add(freeProject);
            await _context.SaveChangesAsync();

            return Ok(freeProject);
        }
    }
}
