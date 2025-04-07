using API.Services;
using DataAccessLayer.Data;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILockoutService _lockoutService;
        private readonly ITokenService _tokenService;
        public AuthenticateController(AppDbContext context, IConfiguration configuration,ILockoutService lockoutService,ITokenService tokenService)
        {
            _context = context;
            _configuration = configuration;
            _lockoutService = lockoutService;
            _tokenService=tokenService;
        }
        [HttpGet("TestConnection")]
        public IActionResult TestConnection()
        {
            try
            {
                var canConnect = _context.Database.CanConnect();
                return Ok(new { Message = canConnect ? "Connected!" : "Cannot connect!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }
        [HttpPost("swaggerlogin")]
         public IActionResult swaggerlogin([FromBody] LoginModel request)
        {
            if (request.Username == "IbrahimAtefSwaggerAdmin" && request.Password == "Ibrahim2000@SwaggerAdminwithyousef_BackEnd")
            {
                Program.IsAuthenticated = true;

                return Ok();
            }

            Program.IsAuthenticated = false;

            return Ok();
        }
        [HttpGet("Login")]
        public async Task<ActionResult<User>> Login(string Email, string password, string imei)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => (x.Phone1 == Email || x.Phone2 == Email || x.SchoolName == Email));

            if (user.SchoolName== null)
            {
                return NotFound("Invalid credentials");
            }
            // تحقق مما إذا كان المستخدم محظورًا
            if (await _lockoutService.IsUserLockedOutAsync(Email))
            {
                return StatusCode(403, "Your account is locked due to multiple failed login attempts. Please try again later.");
            }

            // تحقق من كلمة المرور
            if (user.Password != password)
            {
                // زيادة محاولات الفشل
                await _lockoutService.IncrementFailedAttemptsAsync(Email);

                // قفل الحساب إذا تخطى المستخدم الحد الأقصى للمحاولات الفاشلة
                if (user.FailedLoginAttempts >= 5)
                {
                    await _lockoutService.LockoutUserAsync(Email, 5);
                    await _lockoutService.ResetFailedAttemptsAsync(Email);

                    return StatusCode(403, "Your account is locked due to multiple failed login attempts. Please try again later.");
                }

                return Unauthorized("Invalid credentials");
            }

           
            

                if (user.Imei == null)
                {
                    user.Imei = imei;
                    await _context.SaveChangesAsync();
                }
                else if (user.Imei != imei)
                {
                return Unauthorized("Incorrect device");
                 }
            var accessToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = await GenerateAndSaveRefreshTokenAsync(user);

                return Ok(new { accessToken, refreshToken,user });
            
        }

        [HttpGet("LoginDesktop")]
        public async Task<ActionResult<User>> LoginDesktop(string Email, string password, string imeiDesktop)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => (x.Phone1 == Email || x.Phone2 == Email || x.SchoolName == Email));

            if (user.SchoolName == null)
            {
                return NotFound("Invalid credentials");
            }
            // تحقق مما إذا كان المستخدم محظورًا
            if (await _lockoutService.IsUserLockedOutAsync(Email))
            {
                return StatusCode(403, "Your account is locked due to multiple failed login attempts. Please try again later.");
            }

            // تحقق من كلمة المرور
            if (user.Password != password)
            {
                // زيادة محاولات الفشل
                await _lockoutService.IncrementFailedAttemptsAsync(Email);

                // قفل الحساب إذا تخطى المستخدم الحد الأقصى للمحاولات الفاشلة
                if (user.FailedLoginAttempts >= 5)
                {
                    await _lockoutService.LockoutUserAsync(Email, 5);
                    await _lockoutService.ResetFailedAttemptsAsync(Email);

                    return StatusCode(403, "Your account is locked due to multiple failed login attempts. Please try again later.");
                }

                return Unauthorized("Invalid credentials");
            }





            if (string.IsNullOrEmpty(user.Imeidesktop) || user.Imeidesktop.Equals("NULL", StringComparison.OrdinalIgnoreCase))
            {
                user.Imeidesktop = imeiDesktop;
                await _context.SaveChangesAsync();
            }
            else if (user.Imeidesktop != imeiDesktop)
            {
                return Unauthorized("Incorrect IP device");
            }
                var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = await GenerateAndSaveRefreshTokenAsync(user);

            return Ok(new { accessToken, refreshToken, user });

        }
        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            // إنشاء الـ Refresh Token
            var refreshToken = _tokenService.GenerateRefreshToken();

            // إعداد بيانات الـ Refresh Token
            var refreshTokenEntry = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id, // ربط التوكن بالمستخدم
                Expiration = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:RefreshTokenLifespanDays"])), // تعيين مدة صلاحية التوكن
                IsRevoked = false
            };

            // إضافة التوكن إلى قاعدة البيانات
            _context.RefreshTokens.Add(refreshTokenEntry);
            await _context.SaveChangesAsync();

            // إرجاع التوكن
            return refreshToken;
        }
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken(string refreshToken)
        {
            var newAccessToken = await _tokenService.RefreshAccessTokenAsync(refreshToken);

            if (newAccessToken == null)
            {
                return Unauthorized("Invalid or expired refresh token,please login again");
            }

            return Ok(new
            {
                AccessToken = newAccessToken
            });
        }
    

        public class AdminCreateUserDto
        {
            public string Name { get; set; }
            public string Password { get; set; }
            public string SchoolName { get; set; }
            public string Phone1 { get; set; }
            public int Status { get; set; }
            public int UserType { get; set; }

        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout(string refreshToken)
        {
            var tokenEntry = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (tokenEntry != null)
            {
                tokenEntry.IsRevoked = true;
                await _context.SaveChangesAsync();
            }

            return Ok("Logged out successfully.");
        }

        [HttpGet("SayHelloWorld")]
        public string SayHelloWorld()
        {
            return "Hello, World! 5/3/2024 ";

        }

        [Authorize(Policy = "UserType")]
        [HttpPost("UpdateIsPublish")]
        public async Task<IActionResult> UpdateIsPublish( int value)
        {
            try
            {
                if (value != 0 && value != 1)
                {
                    return BadRequest("Invalid input. Please provide 0 or 1.");
                }

                // Fetch the first record
                var publishRecord = await _context.publish.FirstOrDefaultAsync();
                if (publishRecord == null)
                {
                    return NotFound("No publish record found.");
                }

                publishRecord.IsPublish = value == 1 ? "true" : "false";

                // Save the changes
                await _context.SaveChangesAsync();

                // Return the updated value
                return Ok(new { IsPublish = publishRecord.IsPublish });
            }
            catch (Exception ex)
            {
                // Handle any potential errors
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [HttpPost("IsPublish")]
        public async Task<IActionResult> IsPublish()
        {
            // Simply return the value back as a response
            try
            {
                // LINQ query to get the first IsPublish value
                var isPublish = await _context.publish
                                              .Select(p => p.IsPublish)
                                              .FirstOrDefaultAsync();
                return Ok(new { IsPublish = isPublish });
            }
            catch (Exception ex)
            {
                // Handle any potential errors
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("UpdateIsMobileUpdate")]
        public async Task<IActionResult> UpdateIsMobileUpdate(int value)
        {
            try
            {
                if (value != 0 && value != 1)
                {
                    return BadRequest("Invalid input. Please provide 0 or 1.");
                }

                // Fetch the first record
                var publishRecord = await _context.MobileUpdate.FirstOrDefaultAsync();
                if (publishRecord == null)
                {
                    return NotFound("No publish record found.");
                }
                if (value == 1)
                {
                    publishRecord.IsPublish = "true";
                }
                else
                    publishRecord.IsPublish = "false";

                await _context.SaveChangesAsync();

                return Ok(new { IsPublish = publishRecord.IsPublish });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("IsMobileUpdate")]
        public async Task<IActionResult> IsMobileUpdate()
        {
            // Simply return the value back as a response
            try
            {
                // LINQ query to get the first IsPublish value
                var isPublish = await _context.MobileUpdate
                                              .Select(p => p.IsPublish)
                                              .FirstOrDefaultAsync();
                return Ok(new { IsPublish = isPublish });
            }
            catch (Exception ex)
            {
                // Handle any potential errors
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        public class LoginModel
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }

    }


}