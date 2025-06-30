namespace DataAccess.Entities
{
    public class Brand
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Product>? Products { get; set; }
    }
}
