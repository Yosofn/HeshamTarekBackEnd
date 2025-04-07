using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class LectureFile
    {
        [Key]
        public int FileId { get; set; }

        [Required]
        public int LectureId { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        [MaxLength(2083)]
        public string? Url { get; set; }

        // تعريف العلاقة مع جدول Lectures
        [ForeignKey("LectureId")]
        public Lecture? Lecture { get; set; }
    }
}
