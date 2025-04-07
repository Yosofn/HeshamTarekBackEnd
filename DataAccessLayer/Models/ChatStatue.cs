﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Models;

[Index("ChatId", Name = "IX_ChatId")]
public partial class ChatStatue
{
    [Key]
    public int Id { get; set; }

    public int MessageId { get; set; }

    public int RoomId { get; set; }

    public string UserName { get; set; }

    public int UserId { get; set; }

    [Column("isDelivered")]
    public bool IsDelivered { get; set; }

    [Column("isRead")]
    public bool IsRead { get; set; }

    public int ChatId { get; set; }

    [ForeignKey("ChatId")]
    [InverseProperty("ChatStatues")]
    public virtual Chat Chat { get; set; }
}