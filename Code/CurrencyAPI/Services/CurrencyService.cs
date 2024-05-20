using CurrencyAPI.Models;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace CurrencyAPI.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly string BaseUrl = "https://api.frankfurter.app/";

        public CurrencyService(IHttpClientFactory httpClientFactory, IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _httpClient = _httpClientFactory.CreateClient("CurrencyAPI");
            _cache = cache;
        }

        public async Task<CurrencyServiceResponse> CurrencyExchangeRates(string currencyCode)
        {
            string cacheKey = $"CurrencyExchangeRates_{currencyCode.ToUpper()}";

            if (_cache.TryGetValue(cacheKey, out var cachedObj) && cachedObj is CurrencyRates cachedData)
            {
                return new CurrencyServiceResponse { StatusCode = 200, Data = cachedData };
            }

            var response = await GetDataWithRetry($"{BaseUrl}latest?from={currencyCode.ToUpper()}");

            if (!response.IsSuccessStatusCode)
            {
                return new CurrencyServiceResponse { StatusCode = (int)response.StatusCode, Data = response.ReasonPhrase };
            }

            var data = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<CurrencyRates>(data);

            CacheData(cacheKey, result);

            return new CurrencyServiceResponse { StatusCode = 200, Data = result };
        }

        public async Task<CurrencyServiceResponse> CurrencyConvert(decimal amount,string from, string to)
        {
            string cacheKey = $"CurrencyConvert_{amount}_{from.ToUpper()}_{to.ToUpper()}";

            if (_cache.TryGetValue(cacheKey, out var cachedObj) && cachedObj is CurrencyRates cachedData)
            {
                return new CurrencyServiceResponse { StatusCode = 200, Data = cachedData };
            }

            var response = await GetDataWithRetry($"{BaseUrl}latest?amount={amount}&from={from.ToUpper()}&to={to.ToUpper()}");

            if (!response.IsSuccessStatusCode)
            {
                return new CurrencyServiceResponse { StatusCode = (int)response.StatusCode, Data = response.ReasonPhrase };
            }

            var data = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<CurrencyRates>(data);

            CacheData(cacheKey, result);

            return new CurrencyServiceResponse { StatusCode = 200, Data = result };
        }

        public async Task<CurrencyServiceResponse> CurrencyHistoricalRates(DateTime startDate, DateTime endDate, string currencyCode)
        {
            string cacheKey = $"CurrencyHistoricalRates_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}_{currencyCode.ToUpper()}";

            if (_cache.TryGetValue(cacheKey, out var cachedObj) && cachedObj is string cachedData)
            {
                return new CurrencyServiceResponse { StatusCode = 200, Data = cachedData };
            }

            var response = await GetDataWithRetry($"{BaseUrl}{startDate:yyyy-MM-dd}..{endDate:yyyy-MM-dd}?base={currencyCode.ToUpper()}");

            if (!response.IsSuccessStatusCode)
            {
                return new CurrencyServiceResponse { StatusCode = (int)response.StatusCode, Data = response.ReasonPhrase };
            }

            var data = await response.Content.ReadAsStringAsync();

            CacheData(cacheKey, data);

            return new CurrencyServiceResponse { StatusCode = 200, Data = data };
        }

        private async Task<HttpResponseMessage> GetDataWithRetry(string url, int maxRetries = 3)
        {
            int retries = 0;
            HttpResponseMessage response;
            do
            {
                response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                    return response;

                retries++;
            } while (retries < maxRetries);

            return response;
        }

        private void CacheData<T>(string cacheKey, T data)
        {
            if (_cache == null)
            {
                return;
            }

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(GetNextExpiryTime());

            if (cacheEntryOptions == null)
            {
                return;
            }
            _cache.Set(cacheKey, data, cacheEntryOptions);
        }

        private DateTimeOffset GetNextExpiryTime()
        {
            DateTime now = DateTime.UtcNow;
            DateTime today16CET = new DateTime(now.Year, now.Month, now.Day, 14, 0, 0, DateTimeKind.Utc);

            if (now <= today16CET && IsWorkingDay(now))
            {
                return today16CET;
            }

            DateTime nextWorkingDay = now;
            do
            {
                nextWorkingDay = nextWorkingDay.AddDays(1);
            } while (!IsWorkingDay(nextWorkingDay));

            return new DateTime(nextWorkingDay.Year, nextWorkingDay.Month, nextWorkingDay.Day, 14, 0, 0, DateTimeKind.Utc);
        }

        private bool IsWorkingDay(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }
    }
}
