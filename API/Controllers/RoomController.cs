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
    public class RoomController : ControllerBase
    {

            private readonly AppDbContext _context;

            public RoomController(AppDbContext context)
            {
                _context = context;
            }

            [HttpPost("CreateRoom")]
            public async Task<IActionResult> CreateRoom([FromBody] Room room)
            {
                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();
                return Ok("Romm Created sucsessfully");
            }

            [HttpGet("GetCreatedRooms")]
            public async Task<IActionResult> GetCreatedRooms(int hostUserId)
            {
                var rooms = await _context.Rooms.Where(x => x.HostUserId == hostUserId).ToListAsync();
                foreach (var room in rooms)
                {
                    room.UnReadMessages = await _context.ChatStatues
                        .Where(x => x.UserId == hostUserId && x.RoomId == room.Id && x.IsRead == false)
                        .CountAsync();
                }
                return Ok(rooms);
            }

            [HttpGet("GetSubscribedRooms")]
            public async Task<IActionResult> GetSubscribedRooms(int hostUserId)
            {
                var roomUsers = await _context.RoomUsers.Where(x => x.UserId == hostUserId).ToListAsync();
                var subRooms = new List<Room>();

                foreach (var subscripedRoom in roomUsers)
                {
                    var room = await _context.Rooms.FirstOrDefaultAsync(x => x.Id == subscripedRoom.RoomId);
                    if (room == null) continue;

                    room.UnReadMessages = await _context.ChatStatues
                        .Where(x => x.UserId == hostUserId && x.RoomId == subscripedRoom.RoomId && !x.IsRead)
                        .CountAsync();

                    subRooms.Add(room);
                }

                return Ok(subRooms);
            }

            [HttpGet("GetAllRoomsExceptCreated")]
            public async Task<IActionResult> GetAllRoomsExceptCreated(int hostUserId)
            {
                var rooms = await _context.Rooms.Where(x => x.HostUserId != hostUserId).ToListAsync();

                foreach (var room in rooms)
                {
                    room.NumberOfMembers = await _context.RoomUsers.Where(x => x.RoomId == room.Id).CountAsync();
                }

                return Ok(rooms);
            }

            [HttpGet("SearchRooms")]
            public async Task<IActionResult> SearchRooms(string roomName, int hostUserId)
            {
                var rooms = await _context.Rooms.Where(x => x.RoomName.Contains(roomName)
                    && x.HostUserId != hostUserId).ToListAsync();

                return Ok(rooms);
            }

            [HttpPost("UpdateZoomUserPass")]
            public async Task<IActionResult> UpdateZoomUserPass(int roomId, string zoomUser, string zoomPass)
            {
                var room = await _context.Rooms.FindAsync(roomId);
                if (room == null) return NotFound();

                room.ZoomUser = zoomUser;
                room.ZoomPassword = zoomPass;
                await _context.SaveChangesAsync();
                return Ok();
            }

            [HttpDelete("RoomDelete")]
            public async Task<IActionResult> RoomDelete(int roomId)
            {
                var room = await _context.Rooms.FindAsync(roomId);
                if (room == null) return NotFound();

                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
                return Ok();
            }
        }
    }

