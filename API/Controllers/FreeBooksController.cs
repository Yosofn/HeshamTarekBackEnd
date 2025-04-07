using DataAccessLayer.Data;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FreeBooksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public FreeBooksController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/GetFreeBooks
        [HttpGet("GetFreeBooks")]
        public async Task<ActionResult<IEnumerable<FreeBook>>> GetFreeBooks()
        {
            return await _context.FreeBooks.ToListAsync();
        }

        // POST: api/AddFreeVideo
        [HttpPost("AddFreeBook")]
        public async Task<ActionResult<FreeBook>> AddFreeBook(FreeBook freeBook)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.FreeBooks.Add(freeBook);
            await _context.SaveChangesAsync();

            return Ok(freeBook);
        }

        [HttpGet("{id}")]
        public IActionResult GetDocument(int id)
        {
            var documentPath = Path.Combine(_env.WebRootPath, "Books", $"{id}.pdf");
            if (!System.IO.File.Exists(documentPath))
            {
                return NotFound("Document not found");
            }

            var documentFileStream = System.IO.File.OpenRead(documentPath);
            return File(documentFileStream, "application/pdf");
        }

    }
}
