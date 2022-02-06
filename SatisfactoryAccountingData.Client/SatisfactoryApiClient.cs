using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.Extensions.Caching.Memory;
using SatisfactoryAccountingData.Shared.Model;

namespace SatisfactoryAccountingData.Client
{
    public class SatisfactoryApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly ILocalStorageService _localStorage;
        private const string StorageKey = "ReferenceData";

        public SatisfactoryApiClient(HttpClient httpClient, IMemoryCache memoryCache, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _memoryCache = memoryCache;
            _localStorage = localStorage;
        }

        public async Task<SatisfactoryModel> GetSatisfactoryModel()
        {
            var model = await _memoryCache.GetOrCreateAsync(
                $"{nameof(SatisfactoryApiClient)}.{nameof(GetSatisfactoryModel)}",
                async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);

                    if (await _localStorage.ContainKeyAsync(StorageKey))
                    {
                        try
                        {
                            return await _localStorage.GetItemAsync<SatisfactoryModel>(StorageKey);
                        }
                        catch (Exception)
                        {
                            await _localStorage.RemoveItemAsync(StorageKey);
                        }
                    }

                    var model = await _httpClient.GetFromJsonAsync<SatisfactoryModel>(
                        "https://satisfactory-accounting-data.azurewebsites.net/api/get");

                    await _localStorage.SetItemAsync(StorageKey, model);
                    return model;
                });

            if (model == null) throw new Exception("Failed to retrieve JSON model");
            return model;
        }
    }
}
