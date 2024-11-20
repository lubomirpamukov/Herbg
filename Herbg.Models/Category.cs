﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static Herbg.Common.ValidationConstants.Category;
namespace Herbg.Models;

public class Category
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(CategoryNameMaxLength)]
    public string Name { get; set; } = null!;

    [Required]
    public bool IsDeleted { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}