using DataAccessLayer.Models.Enums;

namespace DataAccessLayer.DTOs
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
