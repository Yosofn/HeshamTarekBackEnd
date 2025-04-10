using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class LessonGroups
    {
        public int Id { get; set; } 
        public string Name { get; set; } 

        public string Link { get; set; }

        public int LessonId { get; set; } 

        public string Type { get; set; } 

        public Lesson Lesson { get; set; }
    }
}
