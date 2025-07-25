﻿using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? BriefDescription { get; set; }

    public string? FullDescription { get; set; }

    public string? TechnicalSpecifications { get; set; }

    public decimal Price { get; set; }

    public int? CategoryId { get; set; }
    
    public int? BrandId { get; set; }
    
    public decimal? Rating { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Category? Category { get; set; }

    public virtual ICollection<ProductImage>? ProductImages { get; set; }

    public virtual Brand? Brand { get; set; }
}
