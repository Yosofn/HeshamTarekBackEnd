using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class PackageLesson
    {
        [Key]
        public int PackageLessonId { get; set; }
        [Column("PackageId")]
        public int PackageId { get; set; }
       [ Column("LessonId")]
        public int? LessonId { get; set; }

        // Navigation properties
        [ForeignKey("PackageId")]
        public virtual Package? Package { get; set; }

        [ForeignKey("LessonId")]
        public virtual Lesson? Lesson { get; set; }


    }

}
