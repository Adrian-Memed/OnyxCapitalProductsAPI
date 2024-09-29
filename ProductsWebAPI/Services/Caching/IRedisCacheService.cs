namespace ProductsWebAPI.Services.Caching
{
    public interface IRedisCacheService
    {
        Task<T?> GetData<T>(string key);
        Task SetData<T>(string key, T data);
    }
}
