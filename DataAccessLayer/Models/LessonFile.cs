using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class LessonFile
    {
        [Key]
        public int FileId { get; set; }

        [Required]
        public int LessonId { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        [ForeignKey("LessonId")]
        public Lesson? Lesson { get; set; }
    }
}
