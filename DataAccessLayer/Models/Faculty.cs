﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Models;

public partial class Faculty
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; }

    [InverseProperty("Faculty")]
    public virtual ICollection<Level> Levels { get; set; } = new List<Level>();

    [InverseProperty("Faculty")]
    public virtual ICollection<News> News { get; set; } = new List<News>();

    [InverseProperty("Faculty")]
    public virtual ICollection<Notif> Notifs { get; set; } = new List<Notif>();
}