﻿namespace DataAccess.Entities
{
    public class ProductImage
    {
        public int Id { get; set; }
        public string? ImageUrl { get; set; }
        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }
    }
}
