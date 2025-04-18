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
    public class FeedBacksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FeedBacksController(AppDbContext context)
        {
            _context = context;
        }
        [HttpPost("AddFeedback")]
        public async Task<IActionResult> AddFeedback([FromBody] FeedBackDto feedback)

        {
            if (string.IsNullOrWhiteSpace(feedback.Comment))
            {
                return BadRequest("Feedback comment is required.");
            }

            if (feedback.UserId <= 0)
            {
                return BadRequest("Invalid UserId.");
            }

          

            try
            {
                var NewFeedBack = new Feedback
                {
                    UserId=feedback.UserId,
                    Comment=feedback.Comment,
                    CreatedAt = DateTime.Now,
                    IsSeen = false,

                };
                _context.Feedbacks.Add(NewFeedBack);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Feedback added successfully.", NewFeedBack });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while adding feedback: {ex.Message}");
            }
        }



        [HttpGet("GetUnseenUserResponses/{userId}")]
        public async Task<IActionResult> GetUnseenUserResponses(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("Invalid UserId.");
            }

            var unseenResponses = await _context.Responses
                .Where(r => r.UserId == userId && !r.IsSeen)
                .ToListAsync();

            if (!unseenResponses.Any())
            {
                return NotFound("No unseen responses found for this user.");
            }

            foreach (var response in unseenResponses)
            {
                response.IsSeen = true;
            }

            await _context.SaveChangesAsync();

            return Ok(unseenResponses);
        }


        [HttpGet("GetUserResponses/{userId}")]
        public async Task<IActionResult> GetUserResponses(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("Invalid UserId.");
            }

            var unseenResponses = await _context.Responses
                .Where(r => r.UserId == userId )
                .ToListAsync();

            if (!unseenResponses.Any())
            {
                return NotFound("No unseen responses found for this user.");
            }

            


            return Ok(unseenResponses);
        }


    }
}
