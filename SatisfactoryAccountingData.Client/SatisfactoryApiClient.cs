using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using SatisfactoryAccountingData.Shared.Model;

namespace SatisfactoryAccountingData.Client
{
    public class SatisfactoryApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;

        public SatisfactoryApiClient(HttpClient httpClient, IMemoryCache memoryCache)
        {
            _httpClient = httpClient;
            _memoryCache = memoryCache;
        }

        public async Task<SatisfactoryModel> GetSatisfactoryModel()
        {
            var model = await _memoryCache.GetOrCreateAsync($"{nameof(SatisfactoryApiClient)}.{nameof(GetSatisfactoryModel)}",
                async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30);
                    var model = await _httpClient.GetFromJsonAsync<SatisfactoryModel>(
                        "https://satisfactory-accounting-data.azurewebsites.net/api/get");
                    return model;
                });

            if (model == null) throw new Exception("Failed to retrieve JSON model");
            return model;
        }
    }
}
