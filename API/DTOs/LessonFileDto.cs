using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class LessonFileDto
    {
        [Required]
        public int LessonId { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        [MaxLength(2083)]
        public string? Url { get; set; }
    }
}

