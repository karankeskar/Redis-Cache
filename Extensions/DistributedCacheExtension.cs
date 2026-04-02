using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
namespace RedisCachingDemo.Extensions
{
    public static class DistributedCacheExtension
    {
        public static async Task SetRecordAsync<T>(this IDistributedCache cache, 
        string recordId, //The unique identifier, key 
        T data, // value, that is why it is generic it can be anything
        TimeSpan? absoluteExpireTime = null,
        TimeSpan? unusedExpireTime = null)
        {
            // this gives how long the things will stay in the cache, make them expire, we do not want things sitting in cache indefinitely
            var options = new DistributedCacheEntryOptions();
            //  This gives the value default of 60 seconds, once I put the item in the cache the item will live for 60 seconds, this method forces to give a time. Always make the cache short.
            options.AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromSeconds(60);
            // if we do not use the cache item for sometime, go and get new data when asked again.
            options.SlidingExpiration = unusedExpireTime;
            // So as the data declared or accepted is of any type, this is used to serialize the data in json format, which is a string
            var jsonData = JsonSerializer.Serialize(data);
            // Setting the cache with recordId and jsondata as a key value pair and options=expiration time
            await cache.SetStringAsync(recordId, jsonData, options);
        }

        public static async Task<T> GetRecordAsync<T>(this IDistributedCache cache, string recordId)
        {
            // This takes our record ID, give me the data 
            var jsonData = await cache.GetStringAsync(recordId);
            if(jsonData is null)
            {
                return default(T); // if the model is int it would return 0
            }
            // Gets the string based upon on the recordId, Deserialize that value and return that value.
            return JsonSerializer.Deserialize<T>(jsonData);
        }
    }
}