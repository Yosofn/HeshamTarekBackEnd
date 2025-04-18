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
[Index("UserId", Name = "IX_UserId")]
public partial class Life
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public int MaterialId { get; set; }

    public int? LessonId { get; set; }

    public string Name { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Date { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Time { get; set; }

    public string Comment { get; set; }

    public string ZoomMeetingId { get; set; }

    public string ZoomPassword { get; set; }

    public int Price { get; set; }

    public int? DependancyId { get; set; }

    public int? DependancyType { get; set; }

    [ForeignKey("LessonId")]
    [InverseProperty("Lives")]
    public virtual Lesson Lesson { get; set; }

    [ForeignKey("MaterialId")]
    [InverseProperty("Lives")]
    public virtual Material Material { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Lives")]
    public virtual User User { get; set; }
}