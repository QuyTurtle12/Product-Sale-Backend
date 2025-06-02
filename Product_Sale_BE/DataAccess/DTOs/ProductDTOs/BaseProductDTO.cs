namespace DataAccess.DTOs.ProductDTOs
{
    public class BaseProductDTO
    {
        public string ProductName { get; set; } = string.Empty;
        public string? BriefDescription { get; set; }
        public string? FullDescription { get; set; }
        public string? TechnicalSpecifications { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
    }
}
