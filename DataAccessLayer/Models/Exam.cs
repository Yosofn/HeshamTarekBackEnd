﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Models;

[Index("LessonId", Name = "IX_LessonId")]
[Index("MaterialId", Name = "IX_MaterialId")]
[Index("UnitId", Name = "IX_UnitId")]
[Index("UserId", Name = "IX_UserId")]
public partial class Exam
{
    [Key]
    public int Id { get; set; }

    public string ExamName { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime DateAdded { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime TimeAdded { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime DateEnded { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime TimeEnded { get; set; }

    public int UserId { get; set; }

    public int MaterialId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    public bool Status { get; set; }

    public int Degree { get; set; }

    public int NumberOfQuestions { get; set; }

    [Column("isHomework")]
    public bool IsHomework { get; set; }

    public int? DependancyId { get; set; }

    public int? DependancyType { get; set; }

    public int NumberOfQuestionsToPass { get; set; }

    public int Time { get; set; }

    public bool Repeatable { get; set; }

    public int? UnitId { get; set; }

    public int? LessonId { get; set; }

    [InverseProperty("Exam")]
    public virtual ICollection<ExamDetail> ExamDetails { get; set; } = new List<ExamDetail>();

    [ForeignKey("LessonId")]
    [InverseProperty("Exams")]
    public virtual Lesson Lesson { get; set; }

    [ForeignKey("MaterialId")]
    [InverseProperty("Exams")]
    public virtual Material Material { get; set; }

    [InverseProperty("Exam")]
    public virtual ICollection<Result> Results { get; set; } = new List<Result>();

    [ForeignKey("UnitId")]
    [InverseProperty("Exams")]
    public virtual Unit Unit { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Exams")]
    public virtual User User { get; set; }


    [NotMapped]
    public string DependancyName { get; set; }



    [NotMapped]
    public int NumberOfResults { get; set; }

    [NotMapped]
    public bool isBought { get; set; }
}