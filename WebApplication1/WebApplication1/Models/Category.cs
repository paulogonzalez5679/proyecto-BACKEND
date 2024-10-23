public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<ProductDto> Products { get; set; }
}
