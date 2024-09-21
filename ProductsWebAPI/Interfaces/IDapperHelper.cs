using System.Data;

namespace ProductsWebAPI.Interfaces
{
    public interface IDapperHelper
    {
        Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string sql, object param = null);
        Task<int> ExecuteScalarAsync<T>(IDbConnection connection, string sql, object param = null);
    }
}
