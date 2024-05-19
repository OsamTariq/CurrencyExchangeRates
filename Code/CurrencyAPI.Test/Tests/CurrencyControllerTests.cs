using CurrencyAPI.Controllers;
using CurrencyAPI.Models;
using CurrencyAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
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
            _mockCurrencyService.Setup(x => x.CurrencyExchangeRates("USD")).ReturnsAsync(expectedResponse);

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
            var expectedResponse = "Error in fetching USDa exchange rates.";

            _mockCurrencyService.Setup(x => x.CurrencyExchangeRates("USDa")).ThrowsAsync(new Exception(expectedResponse));

            var result = await _controller.CurrencyExchangeRates("USDa");

            var statusCode = (result as ObjectResult)?.StatusCode;
            var errorMessage = (result as ObjectResult)?.Value as ErrorResponse;

            Assert.That(statusCode, Is.EqualTo(500));
            Assert.IsNotNull(errorMessage);
            Assert.That(expectedResponse, Is.EqualTo(errorMessage.ErrorMessage));
        }

        #endregion


        #region CurrencyConvertTestCases

        [Test]
        public async Task TestCurrencyConvertSuccess()
        {
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
            _mockCurrencyService.Setup(x => x.CurrencyConvert(10, "GBP", "USD")).ReturnsAsync(expectedResponse);

            var result = await _controller.CurrencyConvert(10, "GBP", "USD");

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));

            var actualResponse = okResult.Value as CurrencyRates;
            Assert.IsNotNull(actualResponse);

            Assert.That(actualResponse.Amount, Is.EqualTo(expectedResponse.Amount));
            Assert.That(actualResponse.Base, Is.EqualTo(expectedResponse.Base));

            Assert.That(expectedResponse.Rates, Has.Count.EqualTo(actualResponse?.Rates?.Count));
            foreach (var data in expectedResponse.Rates)
            {
                Assert.IsTrue(actualResponse?.Rates?.ContainsKey(data.Key));
                Assert.That(data.Value, Is.EqualTo(actualResponse?.Rates?[data.Key]));
            }
        }

        [Test]
        public async Task TestCurrencyConvertErrorResponse()
        {
            var expectedResponse = "Currency conversion for the AUD to THB currency is not allowed.";

            _mockCurrencyService.Setup(x => x.CurrencyConvert(10, "AUD", "THB")).ThrowsAsync(new Exception(expectedResponse));

            var result = await _controller.CurrencyConvert(10, "AUD", "THB");

            var statusCode = (result as ObjectResult)?.StatusCode;
            var errorMessage = (result as ObjectResult)?.Value as ErrorResponse;

            Assert.That(statusCode, Is.EqualTo(400));
            Assert.IsNotNull(errorMessage);
            Assert.That(expectedResponse, Is.EqualTo(errorMessage.ErrorMessage));
        }

        [Test]
        public async Task TestCurrencyConvertThrowErrorResponse()
        {
            var expectedResponse = "Error in fetching conversion rates from YPJ currency to AUD";

            _mockCurrencyService.Setup(x => x.CurrencyConvert(10, "YPJ", "AUD")).ThrowsAsync(new Exception(expectedResponse));

            var result = await _controller.CurrencyConvert(10, "YPJ", "AUD");

            var statusCode = (result as ObjectResult)?.StatusCode;
            var errorMessage = (result as ObjectResult)?.Value as ErrorResponse;

            Assert.That(statusCode, Is.EqualTo(500));
            Assert.IsNotNull(errorMessage);
            Assert.That(expectedResponse, Is.EqualTo(errorMessage.ErrorMessage));
        }

        #endregion

        #region HistoricalRatesTestCases

        [Test]
        public async Task TestHistoricalRatesSuccess()
        {
            var startDate = new DateTime(2022, 1, 17);
            var endDate = new DateTime(2024, 1, 15);

            var result = await _controller.CurrencyHistoricalRates(startDate, endDate);

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

            var expectedResponse = new HistoricalRates
            {
                Base = "AUD",
                Start_Date = startDate.ToString(),
                End_Date = endDate.ToString(),
                Rates = new Dictionary<string, Dictionary<string, decimal>>
                {
                    { new DateTime(2020, 01, 14).ToString(), new Dictionary<string, decimal>
                        {
                            { "BGN", 1.214m },
                            { "BRL", 2.8588m },
                            { "CAD", 0.9018m },
                        }
                    },
                    { new DateTime(2020, 01, 15).ToString(), new Dictionary<string, decimal>
                        {
                            { "BGN", 1.2082m },
                            { "BRL", 2.8632m },
                            { "CAD", 0.89974m },
                        }
                    },
                }
            };

            _mockCurrencyService.Setup(x => x.CurrencyHistoricalRates(startDate, endDate, currencyCode)).ReturnsAsync(expectedResponse);

            var result = await _controller.CurrencyHistoricalRates(startDate, endDate, currencyCode);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));

            var actualResponse = okResult.Value as PaginatedResponse<Dictionary<string, decimal>>;
            Assert.IsNotNull(actualResponse);

            Assert.That(expectedResponse.Base, Is.EqualTo(actualResponse.Base));
            Assert.That(expectedResponse.Start_Date, Is.EqualTo(actualResponse.Start_Date));
            Assert.That(expectedResponse.End_Date, Is.EqualTo(actualResponse.End_Date));
        }

        [Test]
        public async Task TestHistoricalRatesErrorResponse()
        {
            var startDate = new DateTime(2020, 01, 14);
            var endDate = new DateTime(2019, 01, 27);
            var currencyCode = "AUD";

            var expectedResponse = "Error in fetching AUD historical rates.";

            _mockCurrencyService.Setup(x => x.CurrencyHistoricalRates(startDate, endDate, currencyCode)).ThrowsAsync(new Exception(expectedResponse));

            var result = await _controller.CurrencyHistoricalRates(startDate, endDate, currencyCode);

            var statusCode = (result as ObjectResult)?.StatusCode;
            var errorMessage = (result as ObjectResult)?.Value as ErrorResponse;

            Assert.That(statusCode, Is.EqualTo(500));
            Assert.IsNotNull(errorMessage);
            Assert.That(expectedResponse, Is.EqualTo(errorMessage.ErrorMessage));
        }

        #endregion
    }
}
