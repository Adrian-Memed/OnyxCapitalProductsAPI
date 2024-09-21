using Dapper;
using ProductsWebAPI.Interfaces;
using System.Data;

namespace ProductsWebAPI.Helper
{
    public class DapperHelper : IDapperHelper
    {
        public async Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string sql, object param = null)
        {
            return await connection.QueryAsync<T>(sql, param);
        }

        public async Task<int> ExecuteScalarAsync<T>(IDbConnection connection, string sql, object param = null)
        {
            return await connection.ExecuteScalarAsync<int>(sql, param);
        }
    }
}
