using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models.Enums
{
    public enum EnumUserStatus
    {
        [Display(Name = "منتظم")]
        Active = 0,
        [Display(Name = "غير منتظم")]
        InActive = 1
    }
}
