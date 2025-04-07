using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models.Enums
{
    public enum EnumPaymentDirection
    {
        [Display(Name = "وارد")]
        Income = 0,
        [Display(Name = "صادر")]
        Outcome = 1
    }
}
