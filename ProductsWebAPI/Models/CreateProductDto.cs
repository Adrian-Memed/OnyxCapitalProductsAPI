namespace ProductsWebAPI.Models
{
    public class CreateProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Colour { get; set; }
        public decimal Price { get; set; }
    }
}
