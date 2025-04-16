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
    public class FreeFilesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FreeFilesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetFreeFiles")]
        public async Task<ActionResult<IEnumerable<FreeFile>>> GetFreeFiles()
        {
            return await _context.FreeFiles.ToListAsync();
        }
       

        [HttpGet("SearchFreeFiles")]
        public async Task<IActionResult> SearchFreeFiles([FromQuery] string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                var freeFiles = await _context.FreeFiles

                    .ToListAsync();
                return Ok(freeFiles);

            }
            else
            {
                var FreeFiles = await _context.FreeFiles
                    .Where(v => (v.Title != null && v.Title.ToLower().Contains(query.ToLower())) ||
                                (v.Description != null && v.Description.ToLower().Contains(query.ToLower())))
                    .ToListAsync();

                if (!FreeFiles.Any())
                {
                    return NotFound("No Files found matching the search query.");
                }

                return Ok(FreeFiles);
            }
        }

        [Authorize(Policy = "UserType")]

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

         [Authorize(Policy = "UserType")]

        [HttpPut("UpdateFreeFiles/{id}")]
        public async Task<IActionResult> UpdateFreeFiles(int id, [FromBody] FreeFile FreeFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingFile = await _context.FreeFiles.FindAsync(id);
            if (existingFile == null)
            {
                return NotFound("FreeFile not found.");
            }

            existingFile.Title = FreeFile.Title;
            existingFile.Description = FreeFile.Description;
            existingFile.Url = FreeFile.Url;

            _context.FreeFiles.Update(existingFile);
            await _context.SaveChangesAsync();

            return Ok(existingFile);
        }

     [Authorize(Policy = "UserType")]

        [HttpDelete("DeleteFreeFile/{id}")]
        public async Task<IActionResult> DeleteFreeFile(int id)
        {
            var freeFile = await _context.FreeFiles.FindAsync(id);
            if (freeFile == null)
            {
                return NotFound("FreeFile not found.");
            }

            _context.FreeFiles.Remove(freeFile);
            await _context.SaveChangesAsync();

            return Ok($"FreeFile with ID {id} has been deleted successfully.");
        }
    }
}