using Dapper;
using Microsoft.Data.Sqlite;
using ProductsWebAPI.Helper;
using ProductsWebAPI.Interfaces;
using ProductsWebAPI.Models;
using ProductsWebAPI.Queries;
using System.Data;

namespace ProductsWebAPI.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IDapperHelper _dapperHelper;

        public ProductRepository(IDbConnection dbConnection, IDapperHelper dapperHelper)
        {
            _dbConnection = dbConnection;
            _dapperHelper = dapperHelper;
        }
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            string sql = ProductQueries.GetAllProducts;
            return await RetryPolicyHelper.ExecuteWithRetryAsync(async () =>
            {
                return await _dapperHelper.QueryAsync<Product>(_dbConnection, sql);
            });
        }
        public async Task<IEnumerable<Product>> GetProductsByColourAsync(string colour)
        {
            string sql = ProductQueries.GetProductsByColour;

            return await RetryPolicyHelper.ExecuteWithRetryAsync(async () =>
            {
                IEnumerable<Product> products = await _dapperHelper.QueryAsync<Product>(_dbConnection, sql, new { Colour = colour });

                return products;
            });
        }
        public async Task<int> AddProductAsync(Product product)
        {
            string sql = ProductQueries.InsertProduct;

            if (_dbConnection is SqliteConnection)
            {
                sql += "SELECT last_insert_rowid();";  // Use SQLite-specific query
            }
            else
            {
                sql += "SELECT CAST(SCOPE_IDENTITY() as int);";  // Use SQL Server query
            }

            return await RetryPolicyHelper.ExecuteWithRetryAsync(async () =>
            {
                int newProductId = await _dapperHelper.ExecuteScalarAsync<int>(_dbConnection, sql, product);
                return newProductId;
            });
        }
    }
}
