using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class UsersWithLessons
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string SchoolName { get; set; }
        public string Password { get; set; }
        public string NationalId { get; set; }
        public string IMEI { get; set; }
        public string IMEIDesktop { get; set; }
        public string Country { get; set; }
        public int UserType { get; set; }
        public int Status { get; set; }
        public int  UserStatus { get; set; }
        public string Lessons { get; set; }
        public int RowNum { get; set; }
    }

}
