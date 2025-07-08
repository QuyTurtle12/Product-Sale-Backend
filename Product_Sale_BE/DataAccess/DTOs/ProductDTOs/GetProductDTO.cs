namespace DataAccess.DTOs.ProductDTOs
{
    public class GetProductDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string BriefDescription { get; set; }
        public string FullDescription { get; set; }
        public string TechnicalSpecifications { get; set; }
        public decimal Price { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int? BrandId { get; set; }
        public string BrandName { get; set; }
        public decimal? Rating { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>(); // Thêm danh sách URL ảnh
    }
}