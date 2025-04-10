using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class LessonGroupDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string Link { get; set; }

        [Required]
        public int LessonId { get; set; } 

        [StringLength(50)]
        public string Type { get; set; }
    }
}
