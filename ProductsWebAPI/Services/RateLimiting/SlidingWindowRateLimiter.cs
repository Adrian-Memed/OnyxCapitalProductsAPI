using StackExchange.Redis;

namespace ProductsWebAPI.Services.RateLimiting
{
    public class SlidingWindowRateLimiter
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly int _limit;
        private readonly TimeSpan _window;
        public SlidingWindowRateLimiter(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _limit = 100;       
            _window = TimeSpan.FromSeconds(60);
        }

        public async Task<bool> IsRateLimitedAsync(string ipAddress)
        {
            var redisDb = _redis.GetDatabase();
            var cacheKey = $"rate_limit_{ipAddress}";
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var windowStart = currentTimestamp - (long)_window.TotalSeconds;

            //Remove all entries older than the sliding window
            await redisDb.SortedSetRemoveRangeByScoreAsync(cacheKey, 0, windowStart);

            //Get the current count of requests in the sliding window
            var count = await redisDb.SortedSetLengthAsync(cacheKey);

            if (count >= _limit)
            {
                return true;
            }

            //add the new requests timestamp
            await redisDb.SortedSetAddAsync(cacheKey, currentTimestamp, currentTimestamp);

            await redisDb.KeyExpireAsync(cacheKey, _window);
            
            return false;
        }
    }
}
