using StackExchange.Redis;
using System.Text.Json;

namespace AuthenService.Service
{
    public class RedisService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly string _connectionString;

        public RedisService(string connectionString)
        {
            _connectionString = connectionString;
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _db = _redis.GetDatabase();
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var serializedValue = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, serializedValue, expiry);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _db.StringGetAsync(key);
            if (value.IsNull)
                return default;

            return JsonSerializer.Deserialize<T>(value!);
        }
        public async Task RemoveAsync(string key)
        {
            await _db.KeyDeleteAsync(key);
        }

        public IServer GetServer()
        {
            var endpoint = _redis.GetEndPoints().First();
            return _redis.GetServer(endpoint);
        }

        public async Task<List<T?>> GetAllAsync<T>(string pattern)
        {
            var server = GetServer();
            var keys = server.Keys(pattern: pattern).ToList();
            var result = new List<T?>();

            foreach (var key in keys)
            {
                var value = await GetAsync<T>(key);
                if (value != null)
                {
                    result.Add(value);
                }
            }

            return result;
        }
        public async Task<bool> KeyExistsAsync(string key)
        {
            return await _db.KeyExistsAsync(key);
        }

        public async Task<TimeSpan?> GetTTLAsync(string key)
        {
            return await _db.KeyTimeToLiveAsync(key);
        }
    }
}