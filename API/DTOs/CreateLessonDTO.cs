using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class CreateUpdateLessonDTO
    {


        public int MaterialId { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }
        public int TeacherId { get; set; }

        public string LessonState { get; set; }


        [Column("price")]
        public int Price { get; set; }

        public int LessonPriority { get; set; }

        public string EncryptionCode { get; set; }
        public IFormFile Image { get; set; }

    }
}
