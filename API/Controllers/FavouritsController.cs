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
    public class FavouritsController : ControllerBase
    {

        private readonly AppDbContext _context;

        public FavouritsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("AddToFavorites")]
        public async Task<ActionResult> AddToFavorites([FromBody] FavouriteDto favoriteDto)
        {
            // Check if the user exists
            var user = await _context.Users.FindAsync(favoriteDto.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Check if the course exists
            var course = await _context.Lessons.FindAsync(favoriteDto.CourseId); // Assuming a Course entity exists
            if (course == null)
            {
                return NotFound("Course not found");
            }

            // Check if the course is already in favorites
            var existingFavorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == favoriteDto.UserId && f.CourseId == favoriteDto.CourseId);

            if (existingFavorite != null)
            {
                return Conflict("This course is already in your favorites");
            }

            // Create new favorite
            var favorite = new Favorite
            {
                UserId = favoriteDto.UserId,
                CourseId = favoriteDto.CourseId
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok("Course added to favorites successfully");
        }

        [HttpGet]
        [Route("GetFavorites/{userId}")]
        public async Task<ActionResult<List<Lesson>>> GetFavorites(int userId)
        {
            // Check if user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            // Retrieve the favorites
            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId &&f.Course != null)
        .Select(f => f.Course)
                .ToListAsync();

            // Handle case where no favorites are found
            if (!favorites.Any())
            {
                return NotFound($"No favorites found for user with ID {userId}.");
            }

            return Ok(favorites);
        }
        [HttpDelete("DeleteFavourite")]
        public async Task<IActionResult> DeleteFavourite(int userId, int courseId)
        {
            // Retrieve the favorite by UserId and LessonId
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.CourseId == courseId);

            // Check if the favorite exists
            if (favorite == null)
            {
                return NotFound(new { message = "No favorite found with the given UserId and LessonId" });
            }

            // Remove the favorite
            _context.Favorites.Remove(favorite);

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok(new { message = "Favorite deleted successfully." });
        }



    }
}
