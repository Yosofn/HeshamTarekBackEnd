﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Models;

public partial class User
{
    [Key]
    public int Id { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public string Name { get; set; }

    public string Phone1 { get; set; }

    public string Phone2 { get; set; }

    public int Government { get; set; }

    public string Address { get; set; }

    public string SchoolName { get; set; }

    public int UserType { get; set; }

    public string NationalId { get; set; }

    public int Balance { get; set; }

    public int Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime RegisterDate { get; set; }

    [Column("IMEI")]
    public string Imei { get; set; }

    public string Country { get; set; }

    [Column("StudentID")]
    public string StudentId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ExpireDate { get; set; }

    public int Points { get; set; }

    public int UserStatus { get; set; }

    [Column("IMEIDesktop")]
    public string Imeidesktop { get; set; }

    [Column("IMEIDesktopOnline")]
    public string ImeidesktopOnline { get; set; }

    public string FilePath { get; set; } // مسار الصورة
    public int? FailedLoginAttempts { get; set; }
    public DateTime? LockoutEndTime { get; set; }


    [InverseProperty("User")]
    public virtual ICollection<AnswerLike> AnswerLikes { get; set; } = new List<AnswerLike>();

    [InverseProperty("User")]
    public virtual ICollection<BlockedLecture> BlockedLectures { get; set; } = new List<BlockedLecture>();

    [InverseProperty("User")]
    public virtual ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();

    [InverseProperty("User")]
    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    [InverseProperty("User")]
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    [InverseProperty("User")]
    public virtual ICollection<Homework> Homeworks { get; set; } = new List<Homework>();

    [InverseProperty("User")]
    public virtual ICollection<Lecture> Lectures { get; set; } = new List<Lecture>();

    [InverseProperty("User")]
    public virtual ICollection<Life> Lives { get; set; } = new List<Life>();



    [InverseProperty("User")]
    public virtual ICollection<PaymentDetail> PaymentDetails { get; set; } = new List<PaymentDetail>();

    [InverseProperty("User")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [InverseProperty("User")]
    public virtual ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();

    [InverseProperty("User")]
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    [InverseProperty("User")]
    public virtual ICollection<QuestionAnswer> QuestionAnswers { get; set; } = new List<QuestionAnswer>();

    [InverseProperty("User")]
    public virtual ICollection<Result> Results { get; set; } = new List<Result>();

    [InverseProperty("User")]
    public virtual ICollection<Material> Materials { get; set; } = new List<Material>();
    public ICollection<OTP> OTPs { get; set; } = new List<OTP>();


}