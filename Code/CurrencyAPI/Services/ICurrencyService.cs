using CurrencyAPI.Models;

namespace CurrencyAPI.Services
{
    public interface ICurrencyService
    {
        Task<CurrencyRates> CurrencyExchangeRates(string currencyCode);
        Task<CurrencyRates> CurrencyConvert(decimal amount, string from, string to);
        Task<HistoricalRates> CurrencyHistoricalRates(DateTime startDate, DateTime endDate, string currencyCode);
    }
}
