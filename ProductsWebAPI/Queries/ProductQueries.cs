using System.Drawing;

namespace ProductsWebAPI.Queries
{
    public static class ProductQueries
    {
        public const string GetAllProducts = "SELECT * FROM Products";

        public const string GetProductsByColour = "SELECT * FROM Products WHERE Colour = @Colour";

        public const string InsertProduct = @"
            INSERT INTO Products (Name, Description, Colour, Price) 
            VALUES (@Name, @Description, @Colour, @Price);";
    }
}
