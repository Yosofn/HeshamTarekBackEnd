﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using API.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DataAccessLayer.Models;

[Index("MaterialId", Name = "IX_MaterialId")]
public partial class Lesson
{
    [Key]
    public int Id { get; set; }

    public int MaterialId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }
    public int TeacherId { get; set; }

    public string LessonState { get; set; }


    [Column(TypeName = "datetime")]
    public DateTime Date { get; set; }

    [Column("price")]
    public int Price { get; set; }

    public int LessonPriority { get; set; }

    public string EncryptionCode { get; set; }

    [InverseProperty("Lesson")]
    public virtual ICollection<BlockedLecture> BlockedLectures { get; set; } = new List<BlockedLecture>();

    [InverseProperty("Lesson")]
    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    [InverseProperty("Course")]
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();


    [InverseProperty("Lesson")]
    public virtual ICollection<Homework> Homeworks { get; set; } = new List<Homework>();

    [JsonIgnore]

    [InverseProperty("Lesson")]
    public virtual ICollection<Lecture> Lectures { get; set; } = new List<Lecture>();

    [InverseProperty("Lesson")]
    public virtual ICollection<Life> Lives { get; set; } = new List<Life>();

    [InverseProperty("Lesson")]
    public virtual ICollection<Questions2> Questions2s { get; set; } = new List<Questions2>();

    public virtual ICollection<PackageLesson> PackageLessons { get; set; } = new List<PackageLesson>();


    [NotMapped]
    public bool IsBought { get; set; }

    [NotMapped]
    public string TrainerName { get; set; }

    [NotMapped]
    public int StudentsNo { get; set; }

    [NotMapped]
    public List<LessonApiDto> LessonApiDto { get; set; } = new List<LessonApiDto>();

    public virtual Material Material { get; set; }
    [ForeignKey("TeacherId")]
    public virtual User User { get; set; }

}

