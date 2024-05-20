using CurrencyAPI.Models;
using CurrencyAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CurrencyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;
        private static readonly string[] ExcludedCurrencies = { "TRY", "PLN", "THB", "MXN" };
        ErrorResponse error;

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

                if (data.StatusCode == 200)
                {
                    return Ok(data.Data);
                }

                error = new ErrorResponse
                {
                    ErrorMessage = data?.Data?.ToString()
                };

                return StatusCode((int)data.StatusCode, error);
            }
            catch (Exception)
            {
                error = new ErrorResponse
                {
                    ErrorMessage = "Internal Server Error"
                };
                return StatusCode((int)HttpStatusCode.InternalServerError, error);
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

                if (data.StatusCode == 200)
                {
                    return Ok(data.Data);
                }

                error = new ErrorResponse
                {
                    ErrorMessage = data?.Data?.ToString()
                };

                return StatusCode((int)data.StatusCode, error);
            }
            catch (Exception)
            {
                error = new ErrorResponse
                {
                    ErrorMessage = "Internal Server Error"
                };
                return StatusCode((int)HttpStatusCode.InternalServerError, error);
            }
        }

        [HttpGet("HistoricalRates")]
        public async Task<IActionResult> CurrencyHistoricalRates(DateTime startDate, DateTime endDate, string currencyCode = "EUR", int page = 1, int pageSize = 10)
        {
            try
            {
                var data = await _currencyService.CurrencyHistoricalRates(startDate, endDate, currencyCode);

                if (data.StatusCode == 200)
                {
                    var rates = JsonConvert.DeserializeObject<HistoricalRates>(data.Data.ToString());

                    var pagedRates = rates?.Rates?.Skip((page - 1) * pageSize).Take(pageSize).ToDictionary(pair => pair.Key, pair => pair.Value);

                    var response = new PaginatedResponse<Dictionary<string, decimal>>
                    {
                        Base = rates?.Base,
                        Start_Date = rates?.Start_Date,
                        End_Date = rates?.End_Date,
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = rates != null ? rates.Rates.Count : 0,
                        Rates = pagedRates != null ? pagedRates : null
                    };
                    return Ok(response);
                }

                error = new ErrorResponse
                {
                    ErrorMessage = data?.Data?.ToString()
                };

                return StatusCode((int)data.StatusCode, error);
            }
            catch (Exception)
            {
                error = new ErrorResponse
                {
                    ErrorMessage = "Internal Server Error"
                };
                return StatusCode((int)HttpStatusCode.InternalServerError, error);
            }
        }
    }
}
