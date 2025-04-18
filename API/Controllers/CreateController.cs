using DataAccessLayer;
using DataAccessLayer.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Models;
using System;
using Microsoft.AspNetCore.Http;
using System.Net;
using DataAccessLayer.Data;
using System.Diagnostics.Metrics;
using API.DTOs;
using System.Security.AccessControl;
using API.Services;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreateController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public CreateController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;

        }

        [HttpPost("ExamResultUpdate")]
        public async Task<IActionResult> ExamResultUpdate(int examId, int userId, int result)
        {
            var theResult = await _context.Results.FirstOrDefaultAsync(x => x.ExamId == examId && x.UserId == userId);
            if (theResult == null)
                return NotFound();

            theResult.UserResult = result;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("QuestionAnswerAdd")]
        public async Task<IActionResult> QuestionAnswerAdd([FromBody] QuestionAnswer user)
        {
            _context.QuestionAnswers.Add(user);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("StudentAddBalance")]
        public async Task<IActionResult> StudentAddBalance(int userId, int addedBalance)
        {
            var student = await _context.Users.FindAsync(userId);
            if (student == null)
                return NotFound();

            student.Balance += addedBalance;

            var paymentDetail = new PaymentDetail
            {
                Direction = (int)EnumPaymentDirection.Income,
                PayDate = DateTime.Now,
                Payed = addedBalance,
                UserId = student.Id
            };
            _context.PaymentDetails.Add(paymentDetail);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("StudentRemoveBalance")]
        public async Task<IActionResult> StudentRemoveBalance(int userId, int removedBalance)
        {
            var student = await _context.Users.FindAsync(userId);
            if (student == null)
                return NotFound();

            student.Balance -= removedBalance;

            var paymentDetail = new PaymentDetail
            {
                Direction = (int)EnumPaymentDirection.Outcome,
                PayDate = DateTime.Now,
                Payed = removedBalance,
                UserId = student.Id
            };
            _context.PaymentDetails.Add(paymentDetail);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("ResultAdd")]
        public async Task<IActionResult> ResultAdd([FromBody] Result result)
        {
            var user = await _context.Users.FindAsync(result.UserId);
            if (user == null)
                return NotFound();

            var points = result.UserResult * 10;
            user.Points += points;

            var exam = await _context.Exams.FindAsync(result.ExamId);
            if (exam == null)
                return NotFound();

            var numberOfResultsForStudent = await _context.Results
                .Where(x => x.UserId == result.UserId && x.ExamId == result.ExamId)
                .CountAsync();

            if (exam.Repeatable && numberOfResultsForStudent <= 1)
            {
                _context.Results.Add(result);
                await _context.SaveChangesAsync();
            }
            else if (!exam.Repeatable && numberOfResultsForStudent == 0)
            {
                _context.Results.Add(result);
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpPost("PostAdd")]
        public async Task<IActionResult> PostAdd([FromBody] Post post)
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("PostAnswerAdd")]
        public async Task<IActionResult> PostAnswerAdd([FromBody] Answer post)
        {
            _context.Answers.Add(post);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("PostDelete")]
        public async Task<IActionResult> PostDelete(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                return NotFound();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("UserDelete")]
        public async Task<IActionResult> UserDelete(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("PostLikeAdd")]
        public async Task<IActionResult> PostLikeAdd([FromBody] PostLike body)
        {
            var checkUser = await _context.PostLikes
                .FirstOrDefaultAsync(x => x.UserId == body.UserId && x.PostId == body.PostId);

            if (checkUser == null)
            {
                _context.PostLikes.Add(body);
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpPost("AnswerLikeAdd")]
        public async Task<IActionResult> AnswerLikeAdd([FromBody] AnswerLike body)
        {
            var checkUser = await _context.AnswerLikes
                .FirstOrDefaultAsync(x => x.UserId == body.UserId && x.AnswerId == body.AnswerId);

            if (checkUser == null)
            {
                _context.AnswerLikes.Add(body);
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] createUserDto userDto)
        {
            var checkUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Phone1 == userDto.Phone1 || x.SchoolName == userDto.SchoolName);

            if (string.IsNullOrWhiteSpace(userDto.SchoolName) || string.IsNullOrWhiteSpace(userDto.Password))
            {
                return BadRequest("Email or Password cannot be empty");
            }

            if (checkUser == null)
            {
                var user = new User
                {
                    Username = userDto.Username, 
                    Password = userDto.Password,
                    Name = userDto.Name,
                    Phone1 = userDto.Phone1,
                    Phone2 = userDto.Phone2,
                    Government = userDto.Government,
                    Address = userDto.Address,
            // schoole name is email   ودى اغرب حاجة شفتها فى حياتى !!! بس انا لاقتها كدا والداتا موجودة 
                    SchoolName = "",   // Initialize with an empty string or a default value
                    UserType = userDto.UserType,
                    NationalId = userDto.NationalId,
                    Balance = userDto.Balance,
                    Status = userDto.Status,
                    RegisterDate = userDto.RegisterDate,
                    Imei = userDto.Imei,
                    Country = userDto.Country,
                    StudentId = userDto.StudentId,
                    ExpireDate = userDto.ExpireDate,
                    Points = userDto.Points,
                    UserStatus = userDto.UserStatus,
                    Imeidesktop = userDto.ImeiDesktop,
                    ImeidesktopOnline = userDto.ImeiDesktopOnline,
                    FilePath = userDto.FilePath
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new { UserId = user.Id,userDto,Message = "User created successfully" });
            }

            return BadRequest("User already exists");
        }

        [HttpPost("VerifyEmail")]
        public async Task<IActionResult> VerifyEmail(VerifyOtpDto userDto)
        {

            var otpCode = new Random().Next(100000, 999999).ToString();
            var otp = new OTP
            {
                OTPCode = otpCode,
                ExpiryTime = DateTime.Now.AddMinutes(10),
                UserId=userDto.UserId

            };

             _context.OTPs.Add(otp);
           await _context.SaveChangesAsync();

            // إرسال البريد الإلكتروني
            await _emailService.SendEmailAsync(userDto.Email, "Verify Your Email", $"Your OTP is: {otpCode}");
            return Ok();
        }



        [HttpPost("CheckDTO")]
        public async Task<IActionResult> CheckDTO([FromBody] VerifyOtpDto verifyDto)
        {
            var otp = await _context.OTPs
                .FirstOrDefaultAsync(o => o.OTPCode == verifyDto.OTPCode && o.ExpiryTime > DateTime.Now);

            if (otp == null)
            {
                return BadRequest("Invalid or expired OTP");
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == verifyDto.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.SchoolName = verifyDto.Email;

            await _context.SaveChangesAsync();


            otp.IsUsed = true;
            _context.OTPs.Update(otp);
            await _context.SaveChangesAsync();

            return Ok("User verified and registered successfully.");
        }
        [Authorize]
        [HttpPost("UpdateIMEI")]
        public async Task<IActionResult> UpdateIMEI( int UserId,string Imei)
        {
            var OldUser = _context.Users.Find(UserId);

            if (OldUser != null)
            {
                OldUser.Imei = Imei;
                await _context.SaveChangesAsync();

                return Ok(OldUser);
            }
            return NotFound();

        }

        [HttpPost("PaymentAdd")]
        public async Task<IActionResult> PaymentAdd([FromBody] Payment payment)
        {
            int itemPrice = 0;
            var user = await _context.Users.FindAsync(payment.UserId);
            int itemId = payment.BuyedItemId;
            string itemName = "";

            switch ((int)payment.ProductType)
            {
                case 0:
                    var lecture = await _context.Lectures.FindAsync(itemId);
                    itemPrice = lecture.Price;
                    itemName = lecture.Name;
                    break;
                case 1:
                    var live = await _context.Lives.FindAsync(itemId);
                    itemPrice = live.Price;
                    itemName = live.Name;
                    break;
                case 2:
                    var exam = await _context.Exams.FindAsync(itemId);
                    itemPrice = Convert.ToInt32(exam.Price);
                    itemName = exam.ExamName;
                    break;
                default:
                    return BadRequest("Invalid product type");
            }

            var paymentOp = new Payment
            {
                Payed = itemPrice,
                BuyedItemId = itemId,
                ProductType = payment.ProductType,
                UserId = user.Id,
                BuyDate = DateTime.Now,
                TrainerId = payment.TrainerId,
                Name = itemName
            };
            _context.Payments.Add(paymentOp);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("PaymentCheckWithoutDays")]
        public async Task<IActionResult> PaymentCheckWithoutDays(int userId, int productType, int buyedItemId)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(x => x.UserId == userId
                && x.ProductType == (int)(EnumProductType)productType
                && x.BuyedItemId == buyedItemId);

            return payment != null ? Ok() : Forbid();
        }

        [HttpPost("PaymentCheck")]
        public async Task<IActionResult> PaymentCheck(int userId, int productType, int buyedItemId)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(x => x.UserId == userId
                && x.ProductType == (int)(EnumProductType)productType
                && x.BuyedItemId == buyedItemId);

            if (payment != null)
            {
                if ((DateTime.Now - payment.BuyDate).Days > 7)
                    return Forbid();

                return Ok();
            }

            return Forbid();
        }

        [HttpPost("UserUpdate")]
        public async Task<IActionResult> UserUpdate([FromBody] User user)
        {
            var oldUser = await _context.Users.FindAsync(user.Id);
            if (oldUser == null)
                return NotFound();

            oldUser.Name = user.Name;
            oldUser.Phone1 = user.Phone1;
            oldUser.Phone2 = user.Phone2;
            oldUser.Address = user.Address;
            oldUser.SchoolName = user.SchoolName;

            await _context.SaveChangesAsync();
            return Ok("Sucsess");
        }

        [HttpPost("StudentAddPoints")]
        public async Task<IActionResult> StudentAddPoints(int userId, int addedPoints, EnumProductType contenttype, int contentid, string contentname)
        {
            if (contenttype == EnumProductType.Question)
            {
                var student = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
                if (student == null)
                    return NotFound();

                student.Points += addedPoints;
                await _context.SaveChangesAsync();
                return Ok();
            }
            else
            {
                var payment = await _context.Payments
                    .Where(x => x.UserId == userId
                    && x.ProductType == (int)contenttype && x.BuyedItemId == contentid)
                    .OrderByDescending(x => x.BuyDate)
                    .FirstOrDefaultAsync();

                if (payment == null)
                {
                    var paymentOp = new Payment
                    {
                        Payed = 1,
                        BuyedItemId = contentid,
                        ProductType = (int)contenttype,
                        UserId = userId,
                        BuyDate = DateTime.Now,
                        TrainerId = userId,
                        Name = contentname
                    };
                    _context.Payments.Add(paymentOp);
                    await _context.SaveChangesAsync();

                    var student = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
                    student.Points += addedPoints;
                    await _context.SaveChangesAsync();
                }

                return Ok();
            }

        }
        public class VerifyOtpDto
        {
            public string? OTPCode { get; set; }
            public string Email { get; set; }

            public int UserId { get; set; }
        }

    }
}
