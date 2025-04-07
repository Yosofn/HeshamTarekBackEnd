using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models.Enums
{
    public enum EnumGovernment
    {
        [Display(Name = "الاسكندرية")]
        Egypt = 0,
        [Display(Name = "القاهرة")]
        Cairo = 1,
        [Display(Name = "البحيرة")]
        Behira = 2,
        [Display(Name = "الجيزة")]
        Giza = 3,
        [Display(Name = "اسوان")]
        Aswan = 4,
        [Display(Name = "اسيوط")]
        Asyot = 5,
        [Display(Name = "الاقصر")]
        Luxor = 6,
        [Display(Name = "البحر الاحمر")]
        BahrAhmr = 7,
        [Display(Name = "بني سويف")]
        BaniSwef = 8,
        [Display(Name = "بورسعيد")]
        PortSaid = 9,
        [Display(Name = "جنوب سيناء")]
        SouthSinai = 10,
        [Display(Name = "الاسماعيلية")]
        Ismaelia = 11,
        [Display(Name = "الدقهلية")]
        Dakahlia = 12,
        [Display(Name = "دمياط")]
        Domiat = 13,
        [Display(Name = "سوهاج")]
        Sohag = 14,
        [Display(Name = "السويس")]
        Swis = 15,
        [Display(Name = "الشرقية")]
        Sharkia = 16,
        [Display(Name = "شمال سيناء")]
        NorthSinai = 17,
        [Display(Name = "الغربية")]
        Gharbia = 18,
        [Display(Name = "الفيوم")]
        Fayom = 19,
        [Display(Name = "القليوبية")]
        Kaliobiah = 20,
        [Display(Name = "قنا")]
        Kena = 21,
        [Display(Name = "كفر الشيخ")]
        KafrShiekh = 22,
        [Display(Name = "مطروح")]
        Matroh = 23,
        [Display(Name = "المنوفية")]
        Mnofia = 24,
        [Display(Name = "المنيا")]
        Elmnia = 25,
        [Display(Name = "الوادي الجديد")]
        WadiGded = 26
    }
}
