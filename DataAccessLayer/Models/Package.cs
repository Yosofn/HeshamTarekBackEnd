using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class Package
    {

        [Key]
        public int PackageId { get; set; }
        [Column("Title")]
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? FilePath { get; set; }

        public decimal? Price { get; set; } // السعر الحالي
        public decimal? OriginalPrice { get; set; } // السعر الأصلي

        [NotMapped] // لن يتم تخزينها في قاعدة البيانات
        public int NumberOfLectures { get; set; } // عدد المحاضرات

        [NotMapped] // لن يتم تخزينها في قاعدة البيانات
        public int NumberOfStudents { get; set; } // عدد الطلاب
        public virtual ICollection<PackageLesson> PackageLessons { get; set; } = new List<PackageLesson>();
    }
}
