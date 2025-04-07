using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.DTOs
{
    public class LatestOfferDto
    {
        [StringLength(255)]
        public string? OfferDescription { get; set; }

        [Required]
        [StringLength(255)]
        public string? OfferTitle { get; set; }

        [StringLength(2083)]
        public string? OfferUrl { get; set; }

        public string? FilePath
        {
            get; set;
        }
    }
}
