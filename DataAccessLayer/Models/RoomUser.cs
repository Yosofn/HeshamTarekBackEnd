﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Models;

public partial class RoomUser
{
    [Key]
    public int Id { get; set; }

    public int RoomId { get; set; }

    public int UserId { get; set; }
}