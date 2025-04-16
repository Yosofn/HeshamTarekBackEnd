using API.DTOs;
using Azure.Core;
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
        [HttpGet("SearchFreeBooks")]
        public async Task<IActionResult> SearchFreeBooks([FromQuery] string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                var freeBooks = await _context.FreeBooks

                    .ToListAsync();
                return Ok(freeBooks);

            }
            else
            {
                var freeBook = await _context.FreeBooks
                    .Where(v => (v.Title != null && v.Title.ToLower().Contains(query.ToLower())) ||
                                (v.Description != null && v.Description.ToLower().Contains(query.ToLower())))
                    .ToListAsync();

                if (!freeBook.Any())
                {
                    return NotFound("No Books found matching the search query.");
                }

                return Ok(freeBook);
            }
        }

        // POST: api/AddFreeVideo
        [Authorize(Policy = "UserType")]

        [HttpPost("AddFreeBook")]
        public async Task<ActionResult<FreeBook>> AddFreeBook([FromForm] FreeBookDto freeBookDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Create a new FreeBook entity
            var freeBook = new FreeBook
            {
                Title = freeBookDto.Title,
                Url = freeBookDto.Url,
                Description = freeBookDto.Description
            };

            _context.FreeBooks.Add(freeBook);
            await _context.SaveChangesAsync();

            if (freeBookDto.Book != null && freeBookDto.Book.Length > 0)
            {
                var fileExtension = Path.GetExtension(freeBookDto.Book.FileName); // Extract file extension
                if (fileExtension != ".pdf")
                {
                    return BadRequest("Invalid file format. Only .pdf files are allowed.");
                }

                var bookPath = Path.Combine(_env.WebRootPath, "Books", $"{freeBook.BookId}{fileExtension}");
                Directory.CreateDirectory(Path.GetDirectoryName(bookPath)!);

                try
                {
                    using var stream = new FileStream(bookPath, FileMode.Create);
                    await freeBookDto.Book.CopyToAsync(stream); // Save the file
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"An error occurred while saving the file: {ex.Message}");
                }

            }

            return Ok(freeBook);
        }

        [Authorize(Policy = "UserType")]

        [HttpPut("UpdateFreeBook/{id}")]
        public async Task<IActionResult> UpdateFreeBook(int id, [FromForm] FreeBookDto freeBookDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var freeBook = await _context.FreeBooks.FindAsync(id);
            if (freeBook == null)
            {
                return NotFound("FreeBook not found.");
            }

            freeBook.Title = freeBookDto.Title;
            freeBook.Url = freeBookDto.Url;
            freeBook.Description = freeBookDto.Description;

            _context.FreeBooks.Update(freeBook);
            await _context.SaveChangesAsync();

            if (freeBookDto.Book != null && freeBookDto.Book.Length > 0)
            {
                var fileExtension = Path.GetExtension(freeBookDto.Book.FileName);
                if (fileExtension != ".pdf")
                {
                    return BadRequest("Invalid file format. Only .pdf files are allowed.");
                }

                var bookPath = Path.Combine(_env.WebRootPath, "Books", $"{freeBook.BookId}{fileExtension}");
                Directory.CreateDirectory(Path.GetDirectoryName(bookPath)!);

                try
                {
                    using var stream = new FileStream(bookPath, FileMode.Create);
                    await freeBookDto.Book.CopyToAsync(stream);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"An error occurred while saving the file: {ex.Message}");
                }
            }

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

        [Authorize(Policy = "UserType")]

        [HttpDelete("DeleteFreeBook/{id}")]
        public async Task<IActionResult> DeleteFreeBook(int id)
        {
            var freeBook = await _context.FreeBooks.FindAsync(id);
            if (freeBook == null)
            {
                return NotFound("FreeBook not found.");
            }

            var bookPath = Path.Combine(_env.WebRootPath, "Books", $"{id}.pdf");

            try
            {
                if (System.IO.File.Exists(bookPath))
                {
                    System.IO.File.Delete(bookPath);
                }

                _context.FreeBooks.Remove(freeBook);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Book and associated file deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting: {ex.Message}");
            }
        }
    }
}
