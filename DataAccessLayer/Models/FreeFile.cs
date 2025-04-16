using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class FreeFile
    {
        [Key]
        public int FileId { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        [MaxLength(2083)]
        public string? Url { get; set; }
        public byte[]? freefile { get; set; }

    }
}
