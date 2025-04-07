namespace API.DTOs
{
    public class PackageWithLessonsDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public IFormFile? Image { get; set; }

        public List<int>? LessonIds { get; set; }
    }
}
