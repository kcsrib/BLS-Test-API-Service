// Services/CachedCpiService.cs
using Microsoft.Extensions.Caching.Memory;
using CpiWebService.Models;

namespace CpiWebService.Services
{
    public class CachedCpiService
    {
        private readonly BlsApiService _blsApiService;
        private readonly IMemoryCache _cache;

        public CachedCpiService(BlsApiService blsApiService, IMemoryCache cache)
        {
            _blsApiService = blsApiService;
            _cache = cache;
        }

        public async Task<CpiData> GetCpiData(string seriesId, int year, Month month)
        {
            string cacheKey = "allCpiData";

            // Check if the data is already cached
            if (!_cache.TryGetValue(cacheKey, out List<CpiData> cpiDataList))
            {
                // Fetch all data from the BLS API service
                cpiDataList = await _blsApiService.GetAllCpiData(seriesId);

                // Cache the entire list for 1 day
                _cache.Set(cacheKey, cpiDataList, TimeSpan.FromDays(1));
            }

            // Filter the cached data for the requested year and month
            var filteredCpiData = cpiDataList.FirstOrDefault(data => data.Year == year && data.Month == (int)month);

            return filteredCpiData;
        }
    }
}
