using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class LockoutService : ILockoutService
    {
        private readonly AppDbContext _context;

        public LockoutService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsUserLockedOutAsync(string username)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => (x.Phone1 == username || x.Phone2 == username || x.SchoolName == username));

            if (user == null)
            {
                return false; // إذا كان المستخدم غير موجود
            }

            // تحقق مما إذا كان المستخدم محظورًا
            return user.LockoutEndTime.HasValue && user.LockoutEndTime > DateTime.UtcNow;
        }

        public async Task LockoutUserAsync(string username, int lockoutDurationMinutes)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => (x.Phone1 == username || x.Phone2 == username || x.SchoolName == username));

            if (user == null)
            {
                return; // إذا كان المستخدم غير موجود
            }

            // قفل المستخدم
            user.LockoutEndTime = DateTime.UtcNow.AddMinutes(lockoutDurationMinutes);
            await _context.SaveChangesAsync();
        }

        public async Task ResetFailedAttemptsAsync(string username)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => (x.Phone1 == username || x.Phone2 == username || x.SchoolName == username));

            if (user == null)
            {
                return; // إذا كان المستخدم غير موجود
            }

            // إعادة تعيين محاولات تسجيل الدخول الفاشلة
            user.FailedLoginAttempts = 0;
            await _context.SaveChangesAsync();
        }

        public async Task IncrementFailedAttemptsAsync(string username)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => (x.Phone1 == username || x.Phone2 == username || x.SchoolName == username));

            if (user == null)
            {
                return; // إذا كان المستخدم غير موجود
            }

            // زيادة محاولات تسجيل الدخول الفاشلة
            user.FailedLoginAttempts++;
            await _context.SaveChangesAsync();
        }
    }

}
