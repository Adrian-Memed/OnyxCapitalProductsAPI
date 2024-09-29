using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ProductsWebAPI.Services.Caching
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDistributedCache _cache;
        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }
        public async Task <T?> GetData<T>(string key)
        {
            var data = await _cache.GetStringAsync(key);

            if (data is null)
                return default(T);

            return JsonSerializer.Deserialize<T>(data);
        }
        public async Task SetData<T>(string key, T data)
        {
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            await _cache.SetStringAsync(key, JsonSerializer.Serialize(data), options);
        }
         
    }
}
