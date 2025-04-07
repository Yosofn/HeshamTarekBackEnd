using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class LessonDetails
    {
        public int LessonId { get; set; }
        public int MaterialId { get; set; }
        public string? MaterialName { get; set; }
        public string? LessonName { get; set; }
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public int Price { get; set; }
        public int LessonPriority { get; set; }
        public int StudentCount { get; set; }
         public int LectureCount { get; set; }

        public string? EncryptionCode { get; set; }
        public string? LessonState { get; set; }
        public int TeacherId { get; set; }
        public string? TeacherName { get; set; }
    }

}
