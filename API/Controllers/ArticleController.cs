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
    public class ArticleController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ArticleController(AppDbContext context)
        {
            _context = context;
        }
     

        // GET: api/Article
        [HttpGet("GetArticles")]
        public async Task<IActionResult> GetArticles()
        {
            var articles = await _context.Article.ToListAsync();
            if (articles == null || !articles.Any())
            {
                return NotFound(new { Message = "No articles found." });
            }
            return Ok(articles);
        }

       // [Authorize(Policy = "UserType")]
        [HttpPost("CreateArticle")]
        public async Task<IActionResult> CreateArticle([FromBody] Article article)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Article.Add(article);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetArticles), new { id = article.Id }, article);
        }

        // DELETE: api/Article/{id}
        [Authorize(Policy = "UserType")]
        [HttpDelete("DeleteArticl{id}")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var article = await _context.Article.FindAsync(id);
            if (article == null)
                return NotFound();

            _context.Article.Remove(article);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
