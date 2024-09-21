using Microsoft.Data.SqlClient;
using Polly;
using Polly.Retry;
using System;
using System.Threading.Tasks;

namespace ProductsWebAPI.Helper
{
    /// <summary>
    /// Handles automatic retries for transient SQL errors, specifically to mitigate delays 
    /// caused by Azure SQL Serverless resuming from a paused state. 
    /// Retries the operation up to 3 times with exponential backoff.
    /// </summary>
    public static class RetryPolicyHelper
    {
        private static readonly AsyncRetryPolicy _retryPolicy = Policy
            .Handle<SqlException>()  
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        
        /// <summary>
        /// Executes the given database operation with retry logic applied.
        /// </summary>
        public static async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action)
        {
            return await _retryPolicy.ExecuteAsync(action);
        }
    }
}
