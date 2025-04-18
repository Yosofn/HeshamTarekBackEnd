using API.DTOs;
using DataAccessLayer;
using DataAccessLayer.Data;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using static API.Controllers.AuthenticateController;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Policy = "UserType")]
        [HttpPost("AdminCreateUser")]
        public async Task<IActionResult> AdminCreateUser([FromBody] AdminCreateUserDto userDto)
        {
            var checkUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Phone1 == userDto.Phone1 || x.SchoolName == userDto.SchoolName);

            if (string.IsNullOrWhiteSpace(userDto.SchoolName) || string.IsNullOrWhiteSpace(userDto.Password))
            {
                return BadRequest("SchoolName or Password cannot be empty");
            }

            if (checkUser != null)
            {
                return BadRequest("User already exists");
            }

            var user = new User
            {
                Name = userDto.Name,
                Username= userDto.Name,
                Password = userDto.Password,
                SchoolName = userDto.SchoolName,
                Phone1 = userDto.Phone1,
                Status = userDto.Status,
                UserType = userDto.UserType,
                RegisterDate = DateTime.Now,
                ExpireDate = DateTime.Now,


            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { UserId = user.Id, Message = "User created successfully" });
        }
        
        [HttpPost("AddLessonsForUsers")]
        public async Task<IActionResult> AddLessonsForUsers(int userId, [FromBody] List<int> lessonIds)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            var existingLessonIds = new List<int>();
            var newLessonIds = new List<int>();

            foreach (var lessonId in lessonIds)
            {
                var exists = await _context.BlockedLectures.AnyAsync(bl => bl.UserId == userId && bl.LessonId == lessonId);
                if (exists)
                {
                    existingLessonIds.Add(lessonId);
                }
                else
                {
                    newLessonIds.Add(lessonId);
                }
            }

            if (newLessonIds.Any())
            {
                foreach (var lessonId in newLessonIds)
                {
                    var blockedLecture = new BlockedLecture
                    {
                        UserId = userId,
                        LessonId = lessonId
                    };
                    _context.BlockedLectures.Add(blockedLecture);
                }

                await _context.SaveChangesAsync();
            }

            if (existingLessonIds.Any())
            {
                var existingLessonsMessage = $"Lessons with IDs {string.Join(", ", existingLessonIds)} already exist for the user.";
                return Ok(new { Message = "Lessons added successfully.", ExistingLessons = existingLessonsMessage });
            }
            else
            {
                return Ok("Lessons added successfully.");
            }
        }


        [HttpGet("GetUsersWithBlockedLessons")]
        public async Task<IActionResult> GetUsersWithBlockedLessons(string? searchTerm, int pageNumber, int pageSize)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest("Page number and page size must be greater than 0.");
            }

            var usersQuery = _context.Users
                .Where(u => string.IsNullOrEmpty(searchTerm) ||
                            u.Phone1.Contains(searchTerm) ||
                            u.SchoolName.Contains(searchTerm) ||
                            u.Phone2.Contains(searchTerm))
                .OrderByDescending(u => u.Id)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Phone1,
                    u.Phone2,
                    u.SchoolName,
                    u.Password,
                    u.NationalId,
                    u.Imei,
                    u.Imeidesktop,
                    u.Country,
                    u.UserType,
                    u.Status,
                    u.UserStatus,
                    Lessons = u.BlockedLectures.Select(bl => new { bl.LessonId, bl.Lesson.Name }).ToList()
                });

            var totalUsers = await usersQuery.CountAsync();
            var users = await usersQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                TotalUsers = totalUsers,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Users = users
            });
        }



        [Authorize(Policy = "UserType")]
        [HttpGet("SearchUserWithLessons")]
        public async Task<IActionResult> SearchUserWithLessons(string searchTerm, int pageNumber, int pageSize)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest("Page number and page size must be greater than 0.");
            }

            var usersQuery = _context.Users
                .Where(u => u.Phone1.Contains(searchTerm) || u.SchoolName.Contains(searchTerm) || u.Phone2.Contains(searchTerm))
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Phone1,
                    u.Phone2,
                    u.SchoolName,
                    u.Password,
                    u.NationalId,
                    u.Imei,
                    u.Imeidesktop,
                    u.Country,
                    u.UserType,
                    u.Status,
                    u.UserStatus,
                    Lessons = u.BlockedLectures.Select(bl => new { bl.LessonId, bl.Lesson.Name }).ToList()
                });

            var totalUsers = await usersQuery.CountAsync();
            var users = await usersQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            if (!users.Any())
            {
                return NotFound("No users found matching the search criteria.");
            }
            return Ok(new
            {
                TotalUsers = totalUsers,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Users = users
            });
        }

        ///[Authorize(Policy = "UserType")]
        [HttpGet("GetUserRankPageInfo")]
        public async Task<IActionResult> GetUserRankPageInfo(int userId, int pageSize)
        {
            if (pageSize < 1)
            {
                return BadRequest("Page size must be greater than 0.");
            }

            // ترتيب جميع المستخدمين

            var allUsers = await _context.Users
                             .OrderBy(u => u.Id).ToListAsync();

            // البحث عن المستخدم بناءً على userId
            var user = allUsers.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // حساب ترتيب المستخدم
            var userRank = allUsers.IndexOf(user) + 1;

            // حساب رقم الصفحة بناءً على ترتيب المستخدم وحجم الصفحة
            var pageNumber = (userRank + pageSize - 1) / pageSize;

            // حساب إجمالي عدد الصفحات
            var totalPages = (allUsers.Count + pageSize - 1) / pageSize;

            return Ok(new
            {
                UserRank = userRank,
                PageNumber = pageNumber,
                TotalPages = totalPages
            });
        }



        [Authorize(Policy = "UserType")]
        [HttpGet("GetUsers")]
        public async Task<ActionResult> GetUsers(int pageNumber, int pageSize)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest("Page number and page size must be greater than 0.");
            }

            var totalUsers = await _context.Users.CountAsync();

            var users = await _context.Users
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            var paginationMetadata = new
            {
                totalUsers,
                pageNumber,
                pageSize,
                totalPages
            };

            return Ok(new { users, paginationMetadata });
        }
        [Authorize]
        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.AnswerLikes)
                .Include(u => u.BlockedLectures)
                .Include(u => u.Coupons)
                .Include(u => u.Exams)
                .Include(u => u.Favorites)
                .Include(u => u.Homeworks)
                .Include(u => u.Lectures)
                .Include(u => u.Lives)
                .Include(u => u.PaymentDetails)
                .Include(u => u.Payments)
                .Include(u => u.PostLikes)
                .Include(u => u.Posts)
                .Include(u => u.QuestionAnswers)
                .Include(u => u.Results)
               .Include(u => u.OTPs)
               .Include(u=>u.Feedbacks)
               .Include(u=>u.Responses)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound("This Id is not found");
            }

            bool isTeacher = await _context.Lessons.AnyAsync(l => l.TeacherId == id);
            if (isTeacher)
            {
                return BadRequest("You can't delete this user because they are a teacher.");
            }

            _context.AnswerLikes.RemoveRange(user.AnswerLikes);
            _context.BlockedLectures.RemoveRange(user.BlockedLectures);
            _context.Coupons.RemoveRange(user.Coupons);
            _context.Exams.RemoveRange(user.Exams);
            _context.Favorites.RemoveRange(user.Favorites);
            _context.Homeworks.RemoveRange(user.Homeworks);
            _context.Lectures.RemoveRange(user.Lectures);
            _context.Lives.RemoveRange(user.Lives);
            _context.PaymentDetails.RemoveRange(user.PaymentDetails);
            _context.Payments.RemoveRange(user.Payments);
            _context.PostLikes.RemoveRange(user.PostLikes);
            _context.Posts.RemoveRange(user.Posts);
            _context.QuestionAnswers.RemoveRange(user.QuestionAnswers);
            _context.Results.RemoveRange(user.Results);
            _context.OTPs.RemoveRange(user.OTPs);
            _context.Feedbacks.RemoveRange(user.Feedbacks);
            _context.Responses.RemoveRange(user.Responses);

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully." });
        }


        [Authorize(Policy = "UserType")]

        [HttpGet("GetInstructors")]
        public async Task<IActionResult> GetInstructors()

        {
            try
            {
                var instructors = await _context.Users
                    .Where(u => u.UserType == 1)
                    .ToListAsync();

                return Ok(instructors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching instructors", error = ex.Message });
            }
        }
        [Authorize(Policy = "UserType")]
        [HttpGet("GetStudents")]
        public async Task<IActionResult> GetStudents(int pageNumber, int pageSize)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest("Page number and page size must be greater than 0.");
            }

            var totalUsers = await _context.Users.Where(u => u.UserType == 0)
                    .CountAsync();

            var users = await _context.Users.Where(u => u.UserType == 0)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            var paginationMetadata = new
            {
                totalUsers,
                pageNumber,
                pageSize,
                totalPages
            };

            return Ok(new { users, paginationMetadata });
        }

        [Authorize(Policy = "UserType")]
        [HttpGet("GetPaginatedtest")]
        public async Task<IActionResult> GetPaginatedtest(int pageNumber, int pageSize)
        {
            var users = await _context.Users
                .FromSqlRaw("EXEC GetPaginatedUsersByUserTypeZero @PageNumber = {0}, @PageSize = {1}", pageNumber, pageSize)
                .Select(u => new
                {
                    Status = u.Status,
                    Balance = u.Balance,
                    NationalId = u.NationalId,
                    SchoolName = u.SchoolName,
                    Address = u.Address,
                    UserType = u.UserType,
                    Government = u.Government,
                    Phone2 = u.Phone2,
                    Phone1 = u.Phone1,
                    Password = u.Password,
                    Name = u.Name,
                    Username = u.Username,
                    Id = u.Id,
                    RegisterDate = u.RegisterDate,
                    IMEI = u.Imei,
                    Country = u.Country,
                    ExpireDate = u.ExpireDate,
                    Points = u.Points,
                    IMEIDesktop = u.Imeidesktop,
                    IMEIDesktopOnline = "NULL"  
                })
                .ToListAsync();  

            var totalCount = await _context.Users
           .FromSqlRaw("SELECT COUNT(*) FROM Users WITH (INDEX(IX_Users_UserType_Zero)) WHERE UserType = 0")
          .CountAsync(); 


            var totalPages = Math.Ceiling((double)totalCount / pageSize);

            var paginationMetadata = new
            {
                totalCount,
                pageNumber,
                pageSize,
                totalPages
            };

            return Ok(new { users, paginationMetadata });
        }
        [Authorize]
        [HttpPut("UpdateUser{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            //if (updateUserDto == null)
            //    return BadRequest("Invalid data.");

            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound($"User with ID {id} not found.");

            if (!string.IsNullOrEmpty(updateUserDto.Email))
                user.SchoolName = updateUserDto.Email;

            if (!string.IsNullOrEmpty(updateUserDto.Password))
                user.Password = updateUserDto.Password;

            if (!string.IsNullOrEmpty(updateUserDto.Name))
                user.Name = updateUserDto.Name;

            if (!string.IsNullOrEmpty(updateUserDto.Phone1))
                user.Phone1 = updateUserDto.Phone1;

            if (!string.IsNullOrEmpty(updateUserDto.Address))
                user.Address = updateUserDto.Address;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok("User updated successfully.");
        }


        [Authorize(Policy = "UserType")]
        [HttpPut("AdminUpdateUser{id}")]
        public async Task<IActionResult> AdminUpdateUser(int id, [FromBody] AdminUpdateUserDto updateUserDto)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound($"User with ID {id} not found.");

            if (!string.IsNullOrEmpty(updateUserDto.Email))
                user.SchoolName = updateUserDto.Email;

            if (!string.IsNullOrEmpty(updateUserDto.Password))
                user.Password = updateUserDto.Password;

            if (!string.IsNullOrEmpty(updateUserDto.Name))
                user.Name = updateUserDto.Name;
                user.Username = updateUserDto.Name;
 
            if (!string.IsNullOrEmpty(updateUserDto.Phone1))
                user.Phone1 = updateUserDto.Phone1;

            user.UserType = updateUserDto.UserType;

            if (updateUserDto.Imei == "0")
                user.Imei = null;

            if (updateUserDto.ImeiDesktop == "0")
                user.Imeidesktop = null;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok("User updated successfully.");
        }

        public class AdminUpdateUserDto
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string Name { get; set; }
            public string Phone1 { get; set; }
            public string Imei { get; set; }
            public string ImeiDesktop { get; set; }

            public int UserType { get; set; }
        }

        [HttpPost("UploadProfilePicture")]
        public async Task<IActionResult> UploadProfilePicture([FromForm] IFormFile profilePicture, [FromForm] int userId)
        {
            if (profilePicture == null || profilePicture.Length == 0)
            {
                return BadRequest("Please upload a valid image file.");
            }

            var userImagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UserImages");
            if (!Directory.Exists(userImagesPath))
            {
                Directory.CreateDirectory(userImagesPath);
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var fileExtension = Path.GetExtension(profilePicture.FileName); 
            var fileName = $"{userId}{fileExtension}"; 
            var filePath = Path.Combine(userImagesPath, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await profilePicture.CopyToAsync(fileStream);
            }

            user.FilePath = $"/UserImages/{fileName}";
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Profile picture uploaded successfully.", ImagePath = user.FilePath });
        }



        public class FileUpload
        {
            public IFormFile? FileName { get; set; }
        }



    }
}
