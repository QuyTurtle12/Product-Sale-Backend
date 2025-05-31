namespace DataAccess.DTOs.ProductDTOs
{
    public class GetProductDTO : BaseProductDTO
    {
        public int ProductId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}
