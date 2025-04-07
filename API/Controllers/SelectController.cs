using DataAccessLayer.Models.Enums;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using Microsoft.VisualBasic;
using DataAccessLayer.Data;
using System.Data.SqlTypes;
using API.Services;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SelectController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILockoutService lockoutService;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        public SelectController(AppDbContext context, ILockoutService lockoutService, IConfiguration configuration, ITokenService tokenService)
        {
            _context = context;
            this.lockoutService = lockoutService;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [HttpGet("ResultsGetAll")]
        public async Task<IActionResult> ResultsGetAll(int userId)
        {
            var exams = await _context.Exams.ToListAsync();
            List<Result> items = new List<Result>();

            foreach (var exam in exams)
            {
                var results = await _context.Results
                    .Where(x => x.UserId == userId && x.ExamId == exam.Id)
                    .Include(x => x.Exam)
                    .ToListAsync();

                items.AddRange(results);
            }

            return Ok(items);
        }

        [HttpGet("SettingsGet")]
        public async Task<IActionResult> SettingsGet()
        {
            var item = await _context.Settings.FirstOrDefaultAsync(x => x.Id == 1);
            var currentTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now).AddHours(2);

            item.IsOpen = currentTime.Hour >= item.TimeFrom.Hour && currentTime.Hour <= item.TimeTo.Hour;

            return Ok(item);
        }

        [HttpGet("DependancyCheck")]
        public async Task<IActionResult> DependancyCheck(int productId, int productType)
        {
            var dto = new DependancyDto();

            switch (productType)
            {
                case 0: // Lecture
                    var lecture = await _context.Lectures.FindAsync(productId);
                    if (lecture?.DependancyId != null)
                    {
                        dto = await GetDependancyAsync((int)lecture.DependancyType, lecture.DependancyId);
                    }
                    break;
                case 1: // Live
                    var live = await _context.Lives.FindAsync(productId);
                    if (live?.DependancyId != null)
                    {
                        dto = await GetDependancyAsync(live.DependancyType, live.DependancyId);
                    }
                    break;
                case 2: // Exam
                    var exam = await _context.Exams.FindAsync(productId);
                    if (exam?.DependancyId != null)
                    {
                        dto = await GetDependancyAsync(exam.DependancyType, exam.DependancyId);
                    }
                    break;
                case 3: // Homework
                    var homework = await _context.Homeworks.FindAsync(productId);
                    if (homework?.DependancyId != null)
                    {
                        dto = await GetDependancyAsync(homework.DependancyType, homework.DependancyId);
                    }
                    break;
            }
            return Ok(dto);
        }

        private async Task<DependancyDto> GetDependancyAsync(int? productType, int? dependancyId)
        {
            if (productType == (int)EnumProductType.Lecture)
            {
                var dependancy = await _context.Lectures.FindAsync(dependancyId);
                return new DependancyDto { DependancyName = dependancy?.Name, ProductType = EnumProductType.Lecture };
            }
            if (productType ==(int) EnumProductType.Live)
            {
                var dependancy = await _context.Lives.FindAsync(dependancyId);
                return new DependancyDto { DependancyName = dependancy?.Name, ProductType = EnumProductType.Live };
            }
            if (productType == (int)EnumProductType.Exam)
            {
                var dependancy = await _context.Exams.FindAsync(dependancyId);
                return new DependancyDto { DependancyName = dependancy?.ExamName, ProductType = EnumProductType.Exam };
            }
            if (productType == (int)EnumProductType.File)
            {
                var dependancy = await _context.Homeworks.FindAsync(dependancyId);
                return new DependancyDto { DependancyName = dependancy?.Name, ProductType = EnumProductType.File };
            }
            return new DependancyDto { DependancyName = "No", ProductType = 0 };
        }

        [HttpGet("CouponsCheck")]
        public async Task<IActionResult> CouponsCheck(string name, int userId, int productType, int productId)
        {
            var producttype = (EnumProductType)productType;
            var coupon = await _context.Coupons.FirstOrDefaultAsync(x => x.Name == name);

            if (coupon?.ContentId == productId && coupon?.ContentType ==(int)producttype)
            {
                if (coupon?.UserId == null || coupon.UserId == 0)
                {
                    coupon.UserId = userId;
                    await _context.SaveChangesAsync();
                    return Ok(new CouponApi { ContentId = coupon.ContentId, ContentType = (int)coupon.ContentType });
                }
            }
            return Ok(new CouponApi { ContentId = 0, ContentType = 0 });
        }

        

        [Authorize]

        [HttpGet("LessonsGet")]
        public List<Lesson> LessonsGet(int userId, int materialId, int lessonId)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == userId);
            if (user == null)
            {
                return null;
            }

            IQueryable<Lesson> lessonsQuery = _context.Lessons.Include(x => x.Material);

            if (lessonId > 0)
            {
                lessonsQuery = lessonsQuery.Where(x => x.Id == lessonId).OrderBy(x => x.LessonPriority);
            }
            else if (materialId > 0)
            {
                lessonsQuery = lessonsQuery.Where(x => x.MaterialId == materialId).OrderBy(x => x.LessonPriority);
            }

            var lessons = lessonsQuery.ToList();
            DateTime now = DateTime.UtcNow.AddHours(2);

            foreach (var lesson in lessons)
            {
                var material = lesson.Material;
                var trainer = _context.Users.FirstOrDefault(x => x.Id == material.UserId);
                lesson.TrainerName = trainer?.Username;

                // Check if the lesson is bought by the user
                var blockedLectures = _context.BlockedLectures.Where(x => x.UserId == userId && x.LessonId == lesson.Id).ToList();
                lesson.IsBought = blockedLectures.Any();

                // Set the total number of students
                lesson.StudentsNo = _context.BlockedLectures.Count(x => x.LessonId == lesson.Id);

                // Fetch and set associated lesson content (homeworks, lectures, exams)
                lesson.LessonApiDto = GetLessonApiDto(lesson.Id, now, lesson.IsBought);
            }

            return lessons;
        }

        private List<LessonApiDto> GetLessonApiDto(int lessonId, DateTime now, bool isBought)
        {
            var lessonApiDtoList = new List<LessonApiDto>();

            // Fetch homeworks
            var homeworks = _context.Homeworks.Where(x => x.LessonId == lessonId).ToList();
            lessonApiDtoList.AddRange(homeworks.Select(homework => new LessonApiDto
            {
                ContentId = homework.Id,
                ContentType = EnumProductType.File,
                ContentName = homework.Name,
                ContentPrice = homework.Price,
                IsBought = isBought
            }));

            // Fetch lectures available up to 'now'
            var lectures = _context.Lectures.Where(x => x.LessonId == lessonId && x.Date <= now.Date).OrderBy(x => x.Priority).ToList();
            lessonApiDtoList.AddRange(lectures.Select(lecture => new LessonApiDto
            {
                ContentId = lecture.Id,
                ContentType = EnumProductType.Lecture,
                ContentName = lecture.Name,
                ContentPrice = lecture.Price,
                IsBought = isBought,
                ContentUrl = lecture.Url
            }));

            // Fetch exams available up to 'now'
            var exams = _context.Exams.Where(x => x.LessonId == lessonId).ToList();
            foreach (var exam in exams)
            {
                var dateToCompare = DateTime.Parse($"{exam.DateAdded:yyyy-MM-dd} {exam.TimeAdded:hh:mm tt}");
                if (dateToCompare <= now)
                {
                    lessonApiDtoList.Add(new LessonApiDto
                    {
                        ContentId = exam.Id,
                        ContentType = EnumProductType.Exam,
                        ContentName = exam.ExamName,
                        ContentPrice = exam.Price,
                        IsBought = isBought
                    });
                }
            }

            return lessonApiDtoList;
        }
        

        [HttpGet]
        [Route("GetLessonsWithLectures")]
        public List<Lesson> GetLessonsWithLectures(int userId, int materialId, int lessonId)
        {
            var item = _context.Users.FirstOrDefault(x => x.Id == userId);
            if (item == null || item.SchoolName == item.Phone1)
            {
                return null;
            }

            var lessonsQuery = _context.Lessons.AsQueryable();

            // حالة عندما يكون lessonId غير محدد
            if (lessonId == 0)
            {
                if (materialId != 0)
                {
                    lessonsQuery = lessonsQuery.Where(x => x.MaterialId == materialId).OrderBy(x => x.LessonPriority);
                }

                var lessons = lessonsQuery.Include(x => x.Material).ToList();

                foreach (var lesson in lessons)
                {
                    lesson.IsBought = false;

                    var material = _context.Materials.FirstOrDefault(x => x.Id == lesson.MaterialId);
                    if (material != null)
                    {
                        var user = _context.Users.FirstOrDefault(x => x.Id == material.UserId);
                        lesson.TrainerName = user?.Username;
                    }

                    var blockedLectures = _context.BlockedLectures.Where(x => x.UserId == userId && x.LessonId == lesson.Id).Any();
                    var allBlockedLectures = _context.BlockedLectures.Count(x => x.LessonId == lesson.Id);
                    lesson.StudentsNo = allBlockedLectures;
                    lesson.IsBought = blockedLectures;
                }

                return lessons;
            }
            else
            {
                // الحالة عندما يكون lessonId محدد
                var lessonItems = lessonsQuery.Where(x => x.Id == lessonId).OrderBy(x => x.LessonPriority).ToList();
                var isBought = _context.BlockedLectures.Any(x => x.UserId == userId && x.LessonId == lessonId);

                var lessonApiDtoList = new List<LessonApiDto>();
                DateTime now = DateTime.UtcNow.AddHours(2);

                foreach (var lesson in lessonItems)
                {
                    // التعامل مع الواجبات
                    var homeworks = _context.Homeworks.Where(x => x.LessonId == lesson.Id).ToList();
                    foreach (var homework in homeworks)
                    {
                        lessonApiDtoList.Add(new LessonApiDto
                        {
                            ContentId = homework.Id,
                            ContentType = EnumProductType.File,
                            ContentName = homework.Name,
                            ContentPrice = homework.Price,
                            IsBought = isBought
                        });
                    }

                    // التعامل مع المحاضرات
                    var lectures = _context.Lectures.Where(x => x.LessonId == lesson.Id).OrderBy(x => x.Priority).ToList();
                    foreach (var lecture in lectures)
                    {
                        if (lecture.Date <= now)
                        {
                            lessonApiDtoList.Add(new LessonApiDto
                            {
                                ContentId = lecture.Id,
                                ContentType = EnumProductType.Lecture,
                                ContentName = lecture.Name,
                                ContentPrice = lecture.Price,
                                IsBought = isBought,
                                ContentUrl = lecture.Url
                            });
                        }
                    }

                    // التعامل مع الامتحانات
                    var exams = _context.Exams.Where(x => x.LessonId == lesson.Id).ToList();
                    foreach (var exam in exams)
                    {
                        DateTime examDateTime = exam.DateAdded.Date.Add(exam.TimeAdded.TimeOfDay);
                        if (examDateTime <= now)
                        {
                            lessonApiDtoList.Add(new LessonApiDto
                            {
                                ContentId = exam.Id,
                                ContentType = EnumProductType.Exam,
                                ContentName = exam.ExamName,
                                ContentPrice = exam.Price,
                                IsBought = isBought
                            });
                        }
                    }

                    lesson.LessonApiDto.AddRange(lessonApiDtoList);
                    lessonApiDtoList.Clear();
                }

                return lessonItems;
            }
        }

        [Authorize]

        [HttpGet]
        [Route("LessonsGetAll")]
        public async Task<ActionResult<List<Lesson>>> LessonsGetAll()
        {
            try
            {
                // جلب جميع الدروس من الداتا بيز مع جلب بيانات المادة والمدرب لكل درس
                var lessons = await _context.Lessons
                    .Include(x => x.Material)
                    .ToListAsync();

                foreach (var lesson in lessons)
                {
                    lesson.IsBought = false;

                    // جلب بيانات المادة والمدرب إن وجدت
                    var material = await _context.Materials.FirstOrDefaultAsync(x => x.Id == lesson.MaterialId);
                    if (material != null)
                    {
                        var trainer = await _context.Users.FirstOrDefaultAsync(x => x.Id == material.UserId);
                        lesson.TrainerName = trainer?.Username;
                    }

                    // حساب عدد الطلاب في كل درس
                    lesson.StudentsNo = await _context.BlockedLectures.CountAsync(x => x.LessonId == lesson.Id);
                }

                return Ok(lessons); // إعادة الدروس مع كل البيانات
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ في حالة حدوثه
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }



        [Authorize]


        [HttpGet("LessonsGetIOS")]

        public async Task<ActionResult<List<Lesson>>> LessonsGetIOS(int userId, int materialId, int lessonId, int version)
        {
            if (version < 18)
            {
                return null;
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                return null;
            }

            if (lessonId == 0)
            {
                var items = await _context.Lessons.Include(x => x.Material).ToListAsync();

                if (materialId != 0)
                {
                    items = await _context.Lessons.Include(x => x.Material)
                        .Where(x => x.MaterialId == materialId)
                        .OrderBy(x => x.LessonPriority)
                        .ToListAsync();
                }

                foreach (var lesson in items)
                {
                    lesson.IsBought = await _context.BlockedLectures.AnyAsync(x => x.UserId == userId && x.LessonId == lesson.Id);
                    var material = await _context.Materials.FirstOrDefaultAsync(x => x.Id == lesson.MaterialId);
                    var trainer = await _context.Users.FirstOrDefaultAsync(x => x.Id == material.UserId);

                    lesson.TrainerName = trainer?.Username;
                    lesson.StudentsNo = await _context.BlockedLectures.CountAsync(x => x.LessonId == lesson.Id);
                }

                return items;
            }
            else
            {
                var lessons = await _context.Lessons.Where(x => x.Id == lessonId).OrderBy(x => x.LessonPriority).ToListAsync();

                bool isBought = await _context.BlockedLectures.AnyAsync(x => x.UserId == userId && x.LessonId == lessonId);
                var lessonApiDtoList = new List<DataAccessLayer.DTOs.LessonApiDto>();

                foreach (var lesson in lessons)
                {
                    DateTime now = DateTime.UtcNow.AddHours(2);

                    var homeworks = await _context.Homeworks.Where(x => x.LessonId == lesson.Id).ToListAsync();
                    foreach (var homework in homeworks)
                    {
                        lessonApiDtoList.Add(new DataAccessLayer.DTOs.LessonApiDto
                        {
                            ContentId = homework.Id,
                            ContentType = EnumProductType.File,
                            ContentName = homework.Name,
                            ContentPrice = homework.Price,
                            IsBought = isBought
                        });
                    }

                    var lectures = await _context.Lectures.Where(x => x.LessonId == lesson.Id).OrderBy(x => x.Priority).ToListAsync();
                    foreach (var lecture in lectures)
                    {
                        var dateToCompare = lecture.Date.Date;
                        if (dateToCompare <= now)
                        {
                            lessonApiDtoList.Add(new DataAccessLayer.DTOs.LessonApiDto
                            {
                                ContentId = lecture.Id,
                                ContentType = EnumProductType.Lecture,
                                ContentName = lecture.Name,
                                ContentPrice = lecture.Price,
                                IsBought = isBought,
                                ContentUrl = lecture.Url
                            });
                        }
                    }

                    var exams = await _context.Exams.Where(x => x.LessonId == lesson.Id).ToListAsync();
                    foreach (var exam in exams)
                    {
                        var dateToCompare = Convert.ToDateTime($"{exam.DateAdded:yyyy-MM-dd} {exam.TimeAdded:hh:mm tt}");
                        if (dateToCompare <= now)
                        {
                            lessonApiDtoList.Add(new DataAccessLayer.DTOs.LessonApiDto
                            {
                                ContentId = exam.Id,
                                ContentType = EnumProductType.Exam,
                                ContentName = exam.ExamName,
                                ContentPrice = exam.Price,
                                IsBought = isBought
                            });
                        }
                    }

            //     lesson.LessonApiDto.AddRange(lessonApiDtoList);
                    lessonApiDtoList.Clear();
                    lessonApiDtoList.TrimExcess();
                }

                return lessons;
            }
        }

        [HttpGet("NewsGetAll")]
        public async Task<ActionResult<List<News>>> NewsGetAll()
        {
            var newsItems = await _context.News.OrderByDescending(x => x.Date).ToListAsync();
            return newsItems;
        }


        [HttpGet("ExamDetailGetById")]
        public async Task<ActionResult<IEnumerable<ExamDetail>>> ExamDetailGetById(int examId)
        {
            var exam = await _context.Exams.FindAsync(examId);
            if (exam == null)
            {
                return NotFound();
            }

            var items = await _context.ExamDetails
                .Where(x => x.ExamId == examId)
                .OrderBy(r => Guid.NewGuid())
                .Take(exam.NumberOfQuestions)
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("MaterialsGetAll")]
        public async Task<ActionResult<IEnumerable<Material>>> MaterialsGetAll()
        {
            var items = await _context.Materials.ToListAsync();
            return Ok(items);
        }

        [HttpGet("Login")]
        public async Task<ActionResult<User>> Login(string Email, string password, string imei)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => (x.Phone1 == Email || x.Phone2 == Email || x.SchoolName == Email));

            if (user.SchoolName == null)
            {
                return NotFound("Invalid credentials");
            }
            // تحقق مما إذا كان المستخدم محظورًا
            if (await lockoutService.IsUserLockedOutAsync(Email))
            {
                return Forbid("Your account is locked due to multiple failed login attempts. Please try again later.");
            }

            // تحقق من كلمة المرور
            if (user.Password != password)
            {
                // زيادة محاولات الفشل
                await lockoutService.IncrementFailedAttemptsAsync(Email);

                // قفل الحساب إذا تخطى المستخدم الحد الأقصى للمحاولات الفاشلة
                if (user.FailedLoginAttempts >= 5)
                {
                    await lockoutService.LockoutUserAsync(Email, 5); // قفل الحساب لمدة 15 دقيقة
                    return Forbid("Your account is locked due to multiple failed login attempts. Please try again later.");
                }

                return Unauthorized("Invalid credentials");
            }

            // إعادة تعيين محاولات الفشل بعد الدخول الناجح
            await lockoutService.ResetFailedAttemptsAsync(Email);
            if (user.Phone1 == "01015784331" && user.Password == "123456")
            {
                return Ok(user);

            }
            else
            {

                if (user.Imei == null)
                {
                    user.Imei = imei;
                    await _context.SaveChangesAsync();
                }

                return Ok(user);
            }
        }

        [HttpGet("LoginDesktop")]
        public async Task<ActionResult<User>> LoginDesktop(string username, string password, string imei)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => (x.Phone1 == username || x.Phone2 == username) && x.Password == password);

            if (user == null)
            {
                return NotFound("Invalid credentials");
            }

            if (user.Imeidesktop == null)
            {
                user.Imeidesktop = imei;
                await _context.SaveChangesAsync();
            }

            return Ok(user);
        }

        [HttpGet("LoginDesktop2")]
        public async Task<ActionResult<User>> LoginDesktop2(string username, string password, string imei)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => (x.Phone1 == username || x.Phone2 == username) && x.Password == password);

            if (user == null)
            {
                return NotFound("Invalid credentials");
            }

            if (user.ImeidesktopOnline == null)
            {
                user.ImeidesktopOnline = imei;
                await _context.SaveChangesAsync();
            }

            return Ok(user);
        }

        [HttpGet("TrainersGetAll")]
        public async Task<ActionResult<IEnumerable<User>>> TrainersGetAll()
        {
            var items = await _context.Users
                .Where(x => x.UserType ==(int)EnumUserType.Trainer)
                .ToListAsync();

            return Ok(items);
        }


        [Authorize]
        [HttpGet("LectureGetAll2")]
        public async Task<ActionResult<IEnumerable<Lecture>>> LectureGetAll2(int? materialId, int? trainerId, int userId)
        {
            DateTime now = DateTime.UtcNow.AddHours(2);
            var itemsToReturn = new List<Lecture>();

            List<Lecture> items;
            var blockedLectures = await _context.BlockedLectures
                .Where(x => x.UserId == userId)
                .ToListAsync();
            var blockedLecturesIds = blockedLectures.Select(x => x.LessonId).ToList();

            // الحالة القديمة
            if (trainerId == 0 && materialId == 0)
            {
                items = await _context.Lectures
                   .Include(x => x.User)
                   .Include(x => x.Material)
                   .Where(x => x.Price >= 0)
                   .ToListAsync();
            }
            else if (trainerId == 0)
            {
                items = await _context.Lectures
                   .Where(x => x.MaterialId == materialId)
                   .Include(x => x.User)
                   .Include(x => x.Material)
                   .Where(x => x.Price >= 0)
                   .ToListAsync();
            }
            else if (materialId == 0)
            {
                items = await _context.Lectures
                   .Where(x => x.UserId == trainerId)
                   .Include(x => x.User)
                   .Include(x => x.Material)
                   .Where(x => x.Price >= 0)
                   .ToListAsync();
            }
            else
            {
                items = await _context.Lectures
                   .Where(x => x.MaterialId == materialId && x.UserId == trainerId)
                   .Include(x => x.User)
                   .Include(x => x.Material)
                   .Where(x => x.Price >= 0)
                   .ToListAsync();
            }

            // الجديدة
            foreach (var lecture in items)
            {
                lecture.IsBought = blockedLecturesIds.Contains(lecture.Id);

                if (lecture.DependancyId != null)
                {
                    var dto = GetDependancy(lecture.DependancyType, lecture.DependancyId);
                    lecture.DependancyName = dto.DependancyName;
                    lecture.DependancyType = dto.ProductType;
                }

                var theDate = lecture.Date.ToString("yyyy-MM-dd 23:59:59");
                var dateToCompare = Convert.ToDateTime(theDate);

                if (dateToCompare <= now)
                {
                    itemsToReturn.Add(lecture);
                }
            }

            return Ok(itemsToReturn);
        }
        [Authorize]

        [HttpGet("LectureGetAll")]
        public async Task<ActionResult<IEnumerable<Lecture>>> LectureGetAll(int? materialId, int? trainerId, int userId, int unitId)
        {
            List<Lecture> items = new();

            if (unitId == 0)
            {
                if (trainerId == 0 && materialId == 0)
                {
                    items = await _context.Lectures
                       .Include(x => x.User)
                       .Include(x => x.Material)
                       .Where(x => x.Price <= 0)
                       .ToListAsync();
                }
                else if (trainerId == 0)
                {
                    items = await _context.Lectures
                       .Where(x => x.MaterialId == materialId)
                       .Include(x => x.User)
                       .Include(x => x.Material)
                       .Where(x => x.Price <= 0)
                       .ToListAsync();
                }
                else if (materialId == 0)
                {
                    items = await _context.Lectures
                       .Where(x => x.UserId == trainerId)
                       .Include(x => x.User)
                       .Include(x => x.Material)
                       .Where(x => x.Price <= 0)
                       .ToListAsync();
                }
                else
                {
                    items = await _context.Lectures
                       .Where(x => x.MaterialId == materialId && x.UserId == trainerId)
                       .Include(x => x.User)
                       .Include(x => x.Material)
                       .Where(x => x.Price <= 0)
                       .ToListAsync();
                }
            }

            foreach (var lecture in items)
            {
                var payment = await _context.Payments
                    .Where(x => x.UserId == userId && x.ProductType == (int)EnumProductType.Lecture && x.BuyedItemId == lecture.Id)
                    .OrderByDescending(x => x.BuyDate)
                    .FirstOrDefaultAsync();

                if (payment == null || (DateTime.Now - payment.BuyDate).Days >= 7)
                {
                    lecture.IsBought = false;
                }
                else
                {
                    lecture.IsBought = true;
                }

                if (lecture.DependancyId != null)
                {
                    var dto = GetDependancy(lecture.DependancyType, lecture.DependancyId);
                    lecture.DependancyName = dto.DependancyName;
                    lecture.DependancyType = dto.ProductType;
                }
            }

            return Ok(items);
        }

        [HttpGet("NotifsGetAll")]
        public async Task<ActionResult<IEnumerable<Notif>>> NotifsGetAll()
        {
            var items = await _context.Notifs.ToListAsync();
            return Ok(items);
        }

        [HttpGet("PaymentsGetAll")]
        public async Task<ActionResult<IEnumerable<Payment>>> PaymentsGetAll(int userId)
        {
            var items = await _context.Payments
                .Where(x => x.UserId == userId)
                .ToListAsync();
            return Ok(items);
        }

        [HttpGet("UserGetBalance")]
        public async Task<ActionResult<int>> UserGetBalance(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return Ok(user?.Balance ?? 0);
        }

        [HttpGet("UserGetInfo")]
        public async Task<ActionResult<User>> UserGetInfo(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            return user != null ? Ok(user) : NotFound("User not found");
        }

        // Helper method for dependency
        private DependancyDto GetDependancy(EnumProductType? productType, int? dependancyId)
        {
            if (productType == EnumProductType.Lecture)
            {
                var dependancy = _context.Lectures.Find(dependancyId);
                return new DependancyDto { DependancyName = dependancy?.Name, ProductType = EnumProductType.Lecture };
            }
            if (productType == EnumProductType.Live)
            {
                var dependancy = _context.Lives.Find(dependancyId);
                return new DependancyDto { DependancyName = dependancy?.Name, ProductType = EnumProductType.Live };
            }
            if (productType == EnumProductType.Exam)
            {
                var dependancy = _context.Exams.Find(dependancyId);
                return new DependancyDto { DependancyName = dependancy?.ExamName, ProductType = EnumProductType.Exam };
            }
            if (productType == EnumProductType.File)
            {
                var dependancy = _context.Homeworks.Find(dependancyId);
                return new DependancyDto { DependancyName = dependancy?.Name, ProductType = EnumProductType.File };
            }

            return new DependancyDto { DependancyName = "No", ProductType = 0 };
        }

        [HttpGet("PaymentDetailGet")]
        public async Task<ActionResult<IEnumerable<PaymentDetail>>> PaymentDetailGet(int userId)
        {
            var items = await _context.PaymentDetails
                .Where(x => x.UserId == userId)
                .Include(x => x.User)
                .OrderByDescending(x => x.PayDate)
                .ToListAsync();
            return Ok(items);
        }
        [Authorize]

        [HttpGet("GetLectureById")]
        public async Task<ActionResult<Lecture>> GetLectureById(int id)
        {
            var item = await _context.Lectures.FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                return NotFound("Lecture not found");
            }

            return Ok(item);
        }

        [HttpGet("GetExamById")]
        public async Task<ActionResult<Exam>> GetExamById(int id)
        {
            var item = await _context.Exams.FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                return NotFound("Exam not found");
            }

            item.NumberOfResults = 0;
            return Ok(item);
        }

        [HttpGet("GetExamById2")]
        public async Task<ActionResult<Exam>> GetExamById2(int id, int userId)
        {
            var item = await _context.Exams.FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                return NotFound("Exam not found");
            }

            item.NumberOfResults = await _context.Results
                .Where(x => x.ExamId == item.Id && x.UserId == userId)
                .CountAsync();
            return Ok(item);
        }

        [HttpGet("GetHomeworkById")]
        public async Task<ActionResult<Homework>> GetHomeworkById(int id)
        {
            var item = await _context.Homeworks.FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                return NotFound("Homework not found");
            }

            return Ok(item);
        }

        [HttpGet("StudentsGetAll")]
        public async Task<ActionResult<IEnumerable<User>>> StudentsGetAll(int levelId)
        {
            var items = await _context.Users
                .Where(x => x.UserType ==(int) EnumUserType.Student)
                .OrderByDescending(x => x.Points)
                .Take(50)
                .ToListAsync();
            return Ok(items);
        }

        [HttpGet("PostsGetAll")]
        public async Task<ActionResult<IEnumerable<Post>>> PostsGetAll(int materialId)
        {
            var items = await _context.Posts
                .Where(x => x.MaterialId == materialId)
                .Include(x => x.User)
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            foreach (var post in items)
            {
                post.Likes = await _context.PostLikes
                    .Where(x => x.PostId == post.Id)
                    .CountAsync();
            }

            items = items.OrderByDescending(o => o.Likes).ToList();
            return Ok(items);
        }

        [HttpGet("AnswersGetAll")]
        public async Task<ActionResult<IEnumerable<Answer>>> AnswersGetAll(int postId)
        {
            var items = await _context.Answers
                .Where(x => x.PostId == postId)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            foreach (var answer in items)
            {
                answer.Likes = await _context.AnswerLikes
                    .Where(x => x.AnswerId == answer.Id)
                    .CountAsync();
            }

            items = items.OrderByDescending(o => o.Likes).ToList();
            return Ok(items);
        }
    }
}
    

