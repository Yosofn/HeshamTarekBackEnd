using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class OTP
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string OTPCode { get; set; } = string.Empty; // لا يمكن أن يكون NULL
        public DateTime ExpiryTime { get; set; }
        public bool IsUsed { get; set; } = false; // القيمة الافتراضية
        public DateTime CreatedAt { get; set; } = DateTime.Now; // القيمة الافتراضية

        // العلاقة بين المستخدم و OTP
        public User User { get; set; } = null!;
    }

}
