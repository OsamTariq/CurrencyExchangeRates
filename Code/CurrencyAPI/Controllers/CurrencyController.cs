using CurrencyAPI.Models;
using CurrencyAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;
        private static readonly string[] ExcludedCurrencies = { "TRY", "PLN", "THB", "MXN" };

        public CurrencyController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        [HttpGet("ExchangeRates")]
        public async Task<IActionResult> CurrencyExchangeRates(string currencyCode = "EUR")
        {
            try
            {
                var data = await _currencyService.CurrencyExchangeRates(currencyCode);
                return Ok(data);
            }
            catch (Exception ex)
            {
                ErrorResponse error = new ErrorResponse()
                {
                    ErrorMessage = ex.Message,
                };
                return StatusCode(500, error);
            }
        }

        [HttpGet("CurrencyConvert")]
        public async Task<IActionResult> CurrencyConvert(decimal amount, string from, string to)
        {
            if (ExcludedCurrencies.Contains(from.ToUpper()) || ExcludedCurrencies.Contains(to.ToUpper()))
            {
                ErrorResponse error = new ErrorResponse()
                {
                    ErrorMessage = $"Currency conversion for the {from} to {to} currency is not allowed."
                };
                return StatusCode(400, error);
            }

            try
            {
                var data = await _currencyService.CurrencyConvert(amount, from, to);
                return Ok(data);
            }
            catch (Exception ex)
            {
                ErrorResponse error = new ErrorResponse()
                {
                    ErrorMessage = ex.Message,
                };
                return StatusCode(500, error);
            }
        }

        [HttpGet("HistoricalRates")]
        public async Task<IActionResult> CurrencyHistoricalRates(DateTime startDate, DateTime endDate, string currencyCode = "EUR", int page = 1, int pageSize = 10)
        {
            try
            {
                var rates = await _currencyService.CurrencyHistoricalRates(startDate, endDate, currencyCode);
                var pagedRates = rates?.Rates?.Skip((page - 1) * pageSize).Take(pageSize).ToDictionary(pair => pair.Key, pair => pair.Value);

                var data = new PaginatedResponse<Dictionary<string, decimal>>
                {
                    Base = rates?.Base,
                    Start_Date = rates?.Start_Date,
                    End_Date = rates?.End_Date,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = rates != null ? rates.Rates.Count : 0,
                    Rates = pagedRates
                };
                return Ok(data);
            }
            catch (Exception ex)
            {
                ErrorResponse error = new ErrorResponse()
                {
                    ErrorMessage = ex.Message,
                };
                return StatusCode(500, error);
            }
        }
    }
}
