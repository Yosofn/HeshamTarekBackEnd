using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{

    public class Article
    {
        [Key]
        public int Id { get; set; }  // الـ Id سيكون من نوع INT ويتم تعيينه تلقائيًا باستخدام Identity

        [MaxLength(255)]
        public string Title { get; set; }  // العنوان بحجم أقصى 255 حرفًا

        public string Content { get; set; }  // المحتوى

        [MaxLength(100)]
        public string Author { get; set; }  // اسم الكاتب بحجم أقصى 100 حرف

        [Required]
        public DateTime PublishedDate { get; set; }  // تاريخ النشر مع القيمة الافتراضية تكون التاريخ الحالي

        [Required]
        public bool IsPublished { get; set; }  // حالة النشر (0 أو 1) مع القيمة الافتراضية تكون 0 (غير منشور)
    }
}

