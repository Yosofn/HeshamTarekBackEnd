using DataAccessLayer.Models.Enums;

namespace API.DTOs
{
    public class DependancyDto
    {
        public string? DependancyName { get; set; }

        public EnumProductType ProductType { get; set; }
    }
}
