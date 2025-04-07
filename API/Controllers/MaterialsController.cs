using DataAccessLayer.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialsController : ControllerBase
    {
        private readonly AppDbContext _context;
     
        public MaterialsController (AppDbContext context)
        {
            _context = context;
        }
        [HttpGet("GetMaterialsWithUsers")]
        public async Task<IActionResult> GetMaterialsWithUsers()
        {
            var materials = await _context.Materials
                .Include(m => m.User)
                .Select(m => new
                {
                    MaterialId = m.Id,
                    MaterialName = m.Name,
                    UserName = m.User.Name,
                    UserId = m.User.Id
                })
                .ToListAsync();

            return Ok(materials);
        }

    }
}
