﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Models;

[Index("UserId", Name = "IX_UserId")]
public partial class PaymentDetail
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime PayDate { get; set; }

    public int UserId { get; set; }

    public int Payed { get; set; }

    public int Direction { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("PaymentDetails")]
    public virtual User User { get; set; }
}