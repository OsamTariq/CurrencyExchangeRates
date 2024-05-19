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

        public async Task<CurrencyRates> CurrencyExchangeRates(string currencyCode)
        {
            string cacheKey = $"CurrencyExchangeRates_{currencyCode.ToUpper()}";

            if (_cache.TryGetValue(cacheKey, out var cachedObj) && cachedObj is CurrencyRates cachedData)
            {
                return cachedData;
            }

            var response = await GetDataWithRetry($"{BaseUrl}latest?from={currencyCode.ToUpper()}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error in fetching {currencyCode} exchange rates.");
            }

            var data = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<CurrencyRates>(data);

            CacheData(cacheKey, result);

            return result;
        }

        public async Task<CurrencyRates> CurrencyConvert(decimal amount,string from, string to)
        {
            string cacheKey = $"CurrencyConvert_{amount}_{from.ToUpper()}_{to.ToUpper()}";

            if (_cache.TryGetValue(cacheKey, out var cachedObj) && cachedObj is CurrencyRates cachedData)
            {
                return cachedData;
            }

            var response = await GetDataWithRetry($"{BaseUrl}latest?amount={amount}&from={from.ToUpper()}&to={to.ToUpper()}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error in fetching conversion rates from {from.ToUpper()} currency to {to.ToUpper()}");
            }

            var data = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<CurrencyRates>(data);

            CacheData(cacheKey, result);

            return result;
        }

        public async Task<HistoricalRates> CurrencyHistoricalRates(DateTime startDate, DateTime endDate, string currencyCode)
        {
            string cacheKey = $"CurrencyHistoricalRates_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}_{currencyCode.ToUpper()}";

            if (_cache.TryGetValue(cacheKey, out var cachedObj) && cachedObj is HistoricalRates cachedData)
            {
                return cachedData;
            }

            var response = await GetDataWithRetry($"{BaseUrl}{startDate:yyyy-MM-dd}..{endDate:yyyy-MM-dd}?base={currencyCode.ToUpper()}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error in fetching {currencyCode.ToUpper()} historical rates.");
            }

            var data = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<HistoricalRates>(data);

            CacheData(cacheKey, result);

            return result;
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

        //private DateTimeOffset GetNextExpiryTime()
        //{
        //    DateTime now = DateTime.UtcNow;
        //    DateTime today16CET = new DateTime(now.Year, now.Month, now.Day, 14, 0, 0, DateTimeKind.Utc);

        //    if (now <= today16CET && IsWorkingDay(now))
        //    {
        //        return today16CET;
        //    }

        //    DateTime nextWorkingDay = now;
        //    do
        //    {
        //        nextWorkingDay = nextWorkingDay.AddDays(1);
        //    } while (!IsWorkingDay(nextWorkingDay));

        //    return new DateTime(nextWorkingDay.Year, nextWorkingDay.Month, nextWorkingDay.Day, 14, 0, 0, DateTimeKind.Utc);
        //}

        //private bool IsWorkingDay(DateTime date)
        //{
        //    return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        //}


        private DateTimeOffset GetNextExpiryTime()
        {
            DateTime now = DateTime.UtcNow;
            // Define the CET timezone
            TimeZoneInfo cetZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

            // Calculate today's 16:00 CET in UTC
            DateTime today16CET = TimeZoneInfo.ConvertTimeToUtc(
                new DateTime(now.Year, now.Month, now.Day, 16, 0, 0),
                cetZone);

            // Check if now is before or at today's 16:00 CET and if today is a working day
            if (now <= today16CET && IsWorkingDay(now))
            {
                return new DateTimeOffset(today16CET, TimeSpan.Zero);
            }

            // Find the next working day
            DateTime nextWorkingDay = now;
            do
            {
                nextWorkingDay = nextWorkingDay.AddDays(1);
            } while (!IsWorkingDay(nextWorkingDay));

            // Calculate the next working day's 16:00 CET in UTC
            DateTime nextWorkingDay16CET = TimeZoneInfo.ConvertTimeToUtc(
                new DateTime(nextWorkingDay.Year, nextWorkingDay.Month, nextWorkingDay.Day, 16, 0, 0),
                cetZone);

            return new DateTimeOffset(nextWorkingDay16CET, TimeSpan.Zero);
        }

        // Example IsWorkingDay method
        private bool IsWorkingDay(DateTime date)
        {
            // A simple example assuming working days are Monday to Friday
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }



    }
}
