using API.Services;
using DataAccessLayer.Data;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]


    public class ForgetPasswordController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public ForgetPasswordController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }


        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
        {
            // التحقق من صحة البريد الإلكتروني
            var user = await _context.Users.FirstOrDefaultAsync(u => u.SchoolName == request.Username);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // إنشاء OTP جديد
            var otp = new OTP
            {
                UserId = user.Id,
                OTPCode = new Random().Next(100000, 999999).ToString(), // إنشاء كود 6 أرقام
                ExpiryTime = DateTime.Now.AddMinutes(10) // مدة صلاحية الكود
            };

            _context.OTPs.Add(otp);
            await _context.SaveChangesAsync();

            // إرسال الكود عبر البريد الإلكتروني
            await _emailService.SendEmailAsync(user.SchoolName, "Password Reset OTP", $"Your OTP is: {otp.OTPCode}");

            return Ok( $"User Id is: {otp.UserId}");
        }

        [HttpPost("VerifyOTP")]
        public async Task<IActionResult> VerifyOTP([FromBody] VerifyOTPRequest request)
        {
            // البحث عن الكود في الجدول والتحقق من صحته
            var otp = await _context.OTPs
                .FirstOrDefaultAsync(o => o.UserId == request.UserId && o.OTPCode == request.OTPCode && !o.IsUsed && o.ExpiryTime > DateTime.Now);

            if (otp == null)
            {
                return BadRequest("Invalid or expired OTP.");
            }

            return Ok("OTP verified successfully. Proceed to reset your password.");
        }
   

        [HttpPost("ResetPassword")]

        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            // البحث عن الكود والتحقق من أنه صحيح وغير مستخدم
            var otp = await _context.OTPs
                .FirstOrDefaultAsync(o => o.UserId == request.UserId && o.OTPCode == request.OTPCode && !o.IsUsed);

            if (otp == null || otp.ExpiryTime < DateTime.Now)
            {
                return BadRequest("Invalid or expired OTP.");
            }

            // تحديث كلمة المرور
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.Password = request.NewPassword; // قم بتشفير كلمة المرور هنا إذا لزم الأمر
            _context.Users.Update(user);

            // تحديث حالة OTP
            otp.IsUsed = true;
            _context.OTPs.Update(otp);

            await _context.SaveChangesAsync();

            return Ok("Password reset successfully.");
        }

    }



    public class ForgetPasswordRequest
    {
        public string Username { get; set; } = string.Empty;
    }
    public class ResetPasswordRequest
    {
        public int UserId { get; set; }
        public string OTPCode { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class VerifyOTPRequest
    {
        public int UserId { get; set; }
        public string OTPCode { get; set; } = string.Empty;
    }

}
