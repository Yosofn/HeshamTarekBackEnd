using DataAccessLayer;
using DataAccessLayer.Data;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomUserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoomUserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("JoinRoom")]
        public async Task<IActionResult> JoinRoom([FromBody] RoomUser roomUser)
        {
            _context.RoomUsers.Add(roomUser);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("DeleteRoomUser")]
        public async Task<IActionResult> DeleteRoomUser(int userId, int roomId)
        {
            var roomUser = await _context.RoomUsers
                .Where(x => x.UserId == userId && x.RoomId == roomId)
                .FirstOrDefaultAsync();

            if (roomUser == null)
            {
                return NotFound();
            }

            _context.RoomUsers.Remove(roomUser);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
