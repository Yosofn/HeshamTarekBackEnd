using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class FreeBookDto
    {
        [MaxLength(255)]

        public string Title { get; set; }
        public string? Description { get; set; }

        [MaxLength(2083)]
        public string? Url { get; set; }
        public IFormFile? Book { get; set; }

    }
}
