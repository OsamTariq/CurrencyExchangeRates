using CurrencyAPI.Controllers;
using CurrencyAPI.Models;
using CurrencyAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CurrencyAPI.Tests
{
    [TestFixture]
    public class CurrencyControllerTests
    {
        private CurrencyController _controller;
        private Mock<ICurrencyService> _mockCurrencyService;

        [SetUp]
        public void Setup()
        {
            _mockCurrencyService = new Mock<ICurrencyService>();
            _controller = new CurrencyController(_mockCurrencyService.Object);
        }

        #region ExchangeRateTestCases

        [Test]
        public async Task TestCurrencyExchangeRatesSuccess()
        {
            var currencyServiceResponse = new CurrencyServiceResponse
            {
                StatusCode = 200,
                Data = "Test_Data"
            };

            _mockCurrencyService.Setup(x => x.CurrencyExchangeRates("EUR")).ReturnsAsync(currencyServiceResponse);

            var result = await _controller.CurrencyExchangeRates("EUR");

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task TestCurrencyExchangeRatesSuccessResponse()
        {
            var expectedResponse = new CurrencyRates
            {
                Amount = 1.0m,
                Base = "USD"
            };

            var currencyServiceResponse = new CurrencyServiceResponse
            {
                StatusCode = 200,
                Data = expectedResponse
            };

            _mockCurrencyService.Setup(x => x.CurrencyExchangeRates("USD")).ReturnsAsync(currencyServiceResponse);

            var result = await _controller.CurrencyExchangeRates("USD");

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));

            var actualResponse = okResult.Value as CurrencyRates;
            Assert.IsNotNull(actualResponse);

            Assert.That(actualResponse.Amount, Is.EqualTo(expectedResponse.Amount));
            Assert.That(actualResponse.Base, Is.EqualTo(expectedResponse.Base));
        }

        [Test]
        public async Task TestCurrencyExchangeRatesErrorResponse()
        {
            var currencyServiceResponse = new CurrencyServiceResponse
            {
                StatusCode = 404,
                Data = "not found"
            };

            _mockCurrencyService.Setup(x => x.CurrencyExchangeRates("USDa")).ReturnsAsync(currencyServiceResponse);

            var result = await _controller.CurrencyExchangeRates("USDa");

            var statusCode = (result as ObjectResult)?.StatusCode;
            var errorMessage = (result as ObjectResult)?.Value as ErrorResponse;

            Assert.That(statusCode, Is.EqualTo(404));
            Assert.IsNotNull(errorMessage);
            Assert.That(currencyServiceResponse.Data, Is.EqualTo(errorMessage.ErrorMessage));
        }

        [Test]
        public async Task TestCurrencyExchangeRatesExceptionResponse()
        {
            var expectedResponse = "Internal Server Error";

            _mockCurrencyService.Setup(x => x.CurrencyExchangeRates("EUR")).ThrowsAsync(new Exception(expectedResponse));

            var result = await _controller.CurrencyExchangeRates("EUR");

            var statusCode = (result as ObjectResult)?.StatusCode;
            var errorMessage = (result as ObjectResult)?.Value as ErrorResponse;

            Assert.IsNotNull(statusCode);
            Assert.That(statusCode, Is.EqualTo(500));
            Assert.IsNotNull(errorMessage);
            Assert.That(expectedResponse, Is.EqualTo(errorMessage.ErrorMessage));
        }

        #endregion


        #region CurrencyConvertTestCases

        [Test]
        public async Task TestCurrencyConvertSuccess()
        {
            var currencyServiceResponse = new CurrencyServiceResponse
            {
                StatusCode = 200,
                Data = "Test_Data"
            };

            _mockCurrencyService.Setup(x => x.CurrencyConvert(10, "AUD", "EUR")).ReturnsAsync(currencyServiceResponse);

            var result = await _controller.CurrencyConvert(10,"AUD","EUR");

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task TestCurrencyConvertSuccessResponse()
        {
            var expectedResponse = new CurrencyRates
            {
                Amount = 10.0m,
                Base = "GBP",
                Rates = new Dictionary<string, decimal>
                {
                    { "USD", 12.6557m }
                }
            };

            var currencyServiceResponse = new CurrencyServiceResponse
            {
                StatusCode = 200,
                Data = expectedResponse
            };

            _mockCurrencyService.Setup(x => x.CurrencyConvert(10, "GBP", "USD")).ReturnsAsync(currencyServiceResponse);

            var result = await _controller.CurrencyConvert(10, "GBP", "USD");

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));

            var actualResponse = okResult.Value as CurrencyRates;
            Assert.IsNotNull(actualResponse);

            Assert.That(actualResponse.Amount, Is.EqualTo(expectedResponse.Amount));
            Assert.That(actualResponse.Base, Is.EqualTo(expectedResponse.Base));
        }

        [Test]
        public async Task TestCurrencyConvertExcludedResponse()
        {
            var currencyServiceResponse = new CurrencyServiceResponse
            {
                StatusCode = 400,
                Data = "Currency conversion for the AUD to THB currency is not allowed."
            };

            _mockCurrencyService.Setup(x => x.CurrencyConvert(10, "AUD", "THB")).ReturnsAsync(currencyServiceResponse);

            var result = await _controller.CurrencyConvert(10, "AUD", "THB");

            var statusCode = (result as ObjectResult)?.StatusCode;
            var errorMessage = (result as ObjectResult)?.Value as ErrorResponse;

            Assert.That(statusCode, Is.EqualTo(400));
            Assert.IsNotNull(errorMessage);
            Assert.That(currencyServiceResponse.Data, Is.EqualTo(errorMessage.ErrorMessage));
        }

        [Test]
        public async Task TestCurrencyConvertErrorResponse()
        {
            var currencyServiceResponse = new CurrencyServiceResponse
            {
                StatusCode = 404,
                Data = "not found"
            };

            _mockCurrencyService.Setup(x => x.CurrencyConvert(10, "YPJ", "AUD")).ReturnsAsync(currencyServiceResponse);

            var result = await _controller.CurrencyConvert(10, "YPJ", "AUD");

            var statusCode = (result as ObjectResult)?.StatusCode;
            var errorMessage = (result as ObjectResult)?.Value as ErrorResponse;

            Assert.That(statusCode, Is.EqualTo(404));
            Assert.IsNotNull(errorMessage);
            Assert.That(currencyServiceResponse.Data, Is.EqualTo(errorMessage.ErrorMessage));
        }

        [Test]
        public async Task TestCurrencyConvertErrorExceptionResponse()
        {
            var expectedResponse = "Internal Server Error";

            _mockCurrencyService.Setup(x => x.CurrencyConvert(10, "YPJ", "AUD")).ThrowsAsync(new Exception(expectedResponse));

            var result = await _controller.CurrencyConvert(10, "YPJ", "AUD");

            var statusCode = (result as ObjectResult)?.StatusCode;
            var errorMessage = (result as ObjectResult)?.Value as ErrorResponse;

            Assert.IsNotNull(statusCode);
            Assert.That(statusCode, Is.EqualTo(500));
            Assert.IsNotNull(errorMessage);
            Assert.That(expectedResponse, Is.EqualTo(errorMessage.ErrorMessage));
        }

        #endregion

        #region HistoricalRatesTestCases

        [Test]
        public async Task TestHistoricalRatesSuccess()
        {
            var startDate = new DateTime(2020, 01, 14);
            var endDate = new DateTime(2020, 01, 27);
            var currencyCode = "AUD";

            var expectedResponse = new PaginatedResponse<Dictionary<string, decimal>>
            {
                Base = "AUD",
                Start_Date = startDate.ToString(),
                End_Date = endDate.ToString(),
                Page = 1,
                PageSize = 1,
                TotalCount = 1,
                Rates = new Dictionary<string, Dictionary<string, decimal>>
                {
                    { new DateTime(2020, 01, 14).ToString(), new Dictionary<string, decimal>
                        {
                            { "BGN", 1.214m },
                        }
                    },
                }
            };

            var currencyServiceResponse = new CurrencyServiceResponse
            {
                StatusCode = 200,
                Data = JsonConvert.SerializeObject(expectedResponse)
            };

            _mockCurrencyService.Setup(x => x.CurrencyHistoricalRates(startDate, endDate, currencyCode)).ReturnsAsync(currencyServiceResponse);

            var result = await _controller.CurrencyHistoricalRates(startDate, endDate, currencyCode);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task TestHistoricalRatesSuccessResponse()
        {
            var startDate = new DateTime(2020, 01, 14);
            var endDate = new DateTime(2020, 01, 27);
            var currencyCode = "AUD";

            var expectedResponse = new PaginatedResponse<Dictionary<string, decimal>>
            {
                Base = "AUD",
                Start_Date = startDate.ToString(),
                End_Date = endDate.ToString(),
                Page = 1,
                PageSize = 1,
                TotalCount = 1,
                Rates = new Dictionary<string, Dictionary<string, decimal>>
                {
                    { new DateTime(2020, 01, 14).ToString(), new Dictionary<string, decimal>
                        {
                            { "BGN", 1.214m },
                            { "BRL", 2.8588m },
                            { "CAD", 0.9018m },
                        }
                    },
                }
            };

            var currencyServiceResponse = new CurrencyServiceResponse
            {
                StatusCode = 200,
                Data = JsonConvert.SerializeObject(expectedResponse)
            };

            _mockCurrencyService.Setup(x => x.CurrencyHistoricalRates(startDate, endDate, currencyCode)).ReturnsAsync(currencyServiceResponse);

            var result = await _controller.CurrencyHistoricalRates(startDate, endDate, currencyCode);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));

            var actualResponse = okResult.Value as PaginatedResponse<Dictionary<string, decimal>>;
            Assert.IsNotNull(actualResponse);

            Assert.That(expectedResponse.Base, Is.EqualTo(actualResponse.Base));
            Assert.That(expectedResponse.Start_Date, Is.EqualTo(actualResponse.Start_Date));
            Assert.That(expectedResponse.End_Date, Is.EqualTo(actualResponse.End_Date));
            Assert.That(actualResponse?.Rates?.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task TestHistoricalRatesErrorResponse()
        {
            var startDate = new DateTime(2020, 01, 14);
            var endDate = new DateTime(2019, 01, 27);
            var currencyCode = "AUD";

            var currencyServiceResponse = new CurrencyServiceResponse
            {
                StatusCode = 404,
                Data = "not found"
            };

            _mockCurrencyService.Setup(x => x.CurrencyHistoricalRates(startDate, endDate, currencyCode)).ReturnsAsync(currencyServiceResponse);

            var result = await _controller.CurrencyHistoricalRates(startDate, endDate, currencyCode);

            var statusCode = (result as ObjectResult)?.StatusCode;
            var errorMessage = (result as ObjectResult)?.Value as ErrorResponse;

            Assert.That(statusCode, Is.EqualTo(404));
            Assert.IsNotNull(errorMessage);
            Assert.That(currencyServiceResponse.Data, Is.EqualTo(errorMessage.ErrorMessage));
        }

        [Test]
        public async Task TestHistoricalRatesExceptionResponse()
        {
            var startDate = new DateTime(2022, 1, 17);
            var endDate = new DateTime(2024, 1, 15);
            var currencyCode = "AUD";

            var expectedResponse = new PaginatedResponse<Dictionary<string, decimal>> { };

            var currencyServiceResponse = new CurrencyServiceResponse
            {
                StatusCode = 200,
                Data = "Internal Server Error"
            };

            _mockCurrencyService.Setup(x => x.CurrencyHistoricalRates(startDate, endDate, currencyCode)).ReturnsAsync(currencyServiceResponse);

            var result = await _controller.CurrencyHistoricalRates(startDate, endDate);

            var statusCode = (result as ObjectResult)?.StatusCode;
            var errorMessage = (result as ObjectResult)?.Value as ErrorResponse;

            Assert.That(statusCode, Is.EqualTo(500));
            Assert.IsNotNull(errorMessage);
            Assert.That(currencyServiceResponse.Data, Is.EqualTo(errorMessage.ErrorMessage));
        }

        #endregion
    }
}
