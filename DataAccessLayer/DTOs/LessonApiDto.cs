using DataAccessLayer.Models.Enums;

namespace API.DTOs
{
    public class LessonApiDto
    {
        public string? ContentName { get; set; }
        public EnumProductType ContentType { get; set; }
        public int ContentId { get; set; }

        public decimal ContentPrice { get; set; }

        public bool IsBought { get; set; }

        public string? ContentUrl { get; set; }

    }

}
