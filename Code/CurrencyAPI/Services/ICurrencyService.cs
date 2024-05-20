using CurrencyAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyAPI.Services
{
    public interface ICurrencyService
    {
        Task<CurrencyServiceResponse> CurrencyExchangeRates(string currencyCode);
        Task<CurrencyServiceResponse> CurrencyConvert(decimal amount, string from, string to);
        Task<CurrencyServiceResponse> CurrencyHistoricalRates(DateTime startDate, DateTime endDate, string currencyCode);
    }
}
