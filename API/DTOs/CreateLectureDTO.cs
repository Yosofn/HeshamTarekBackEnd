using DataAccessLayer.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class CreateUpdateLectureDTO
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Url is required")]
        public string Url { get; set; }
        public string? Drive { get; set; }
        [Required(ErrorMessage = "Priority is required")]
        public int Priority { get; set; }
        public string? LectureDescription { get; set; }
    }

  

}

