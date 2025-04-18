﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Models;

[Index("MaterialId", Name = "IX_MaterialId")]
[Index("UnitId", Name = "IX_UnitId")]
public partial class Question
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; }

    public string Answer1 { get; set; }

    public string Answer2 { get; set; }

    public string Answer3 { get; set; }

    public string Answer4 { get; set; }

    public string AnswerDescription { get; set; }

    public int CorrectAnswerNumber { get; set; }

    public int MaterialId { get; set; }

    public int UnitId { get; set; }

    [ForeignKey("MaterialId")]
    [InverseProperty("Questions")]
    public virtual Material Material { get; set; }

    [InverseProperty("Question")]
    public virtual ICollection<QuestionAnswer> QuestionAnswers { get; set; } = new List<QuestionAnswer>();

    [ForeignKey("UnitId")]
    [InverseProperty("Questions")]
    public virtual Unit Unit { get; set; }
}