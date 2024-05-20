# Currency API Documentation

## Overview
This Currency API allows you to retrieve current exchange rates, convert currencies, and get historical exchange rates. It also includes caching to improve performance and reduce the number of requests to the external API.

## Controllers

### CurrencyController
This controller provides endpoints for accessing currency exchange rates, converting currencies, and retrieving historical rates.

**Endpoints:**
1. `GET /api/Currency/ExchangeRates`
2. `GET /api/Currency/CurrencyConvert`
3. `GET /api/Currency/HistoricalRates`

## Services

### CurrencyService
This service handles the interaction with the external currency API and includes caching for performance optimization.

## Models

### CurrencyRates
Represents the exchange rates for a specific currency.

### HistoricalRates
Represents historical exchange rates over a date range.

### ErrorResponse
Represents an error response.

### PaginatedResponse<T>
Represents a paginated response for historical rates.

## Endpoint Details

### `GET /api/Currency/ExchangeRates`
Fetches the current exchange rates for a specified currency.

**Parameters:**
- `currencyCode` (string): The currency code for which to fetch exchange rates (default: "EUR").

**Response:**
- `200 OK`: Returns the exchange rates.
- `500 Internal Server Error`: Returns an error response.

### `GET /api/Currency/CurrencyConvert`
Converts an amount from one currency to another.

**Parameters:**
- `amount` (decimal): The amount to convert.
- `from` (string): The source currency code.
- `to` (string): The target currency code.

**Response:**
- `200 OK`: Returns the conversion result.
- `400 Bad Request`: Returns an error response if the conversion involves excluded currencies.
- `500 Internal Server Error`: Returns an error response if there's an exception.

### `GET /api/Currency/HistoricalRates`
Fetches historical exchange rates over a specified date range.

**Parameters:**
- `startDate` (DateTime): The start date for the historical data.
- `endDate` (DateTime): The end date for the historical data.
- `currencyCode` (string): The currency code for which to fetch historical rates (default: "EUR").
- `page` (int): The page number for pagination (default: 1).
- `pageSize` (int): The number of items per page (default: 10).

**Response:**
- `200 OK`: Returns the historical rates.
- `500 Internal Server Error`: Returns an error response.

## Caching
As per Frankfurter API documentation, the Frankfurter API tracks foreign exchange reference rates published by the European Central Bank. The data refreshes around 16:00 CET every working day. To optimize performance and reduce the number of requests to the external API, caching is implemented. The cache is set to expire at 16:00 CET every working day, ensuring that updated rates are fetched daily. On weekends, the cached data from the last working day is used since no new rates are published during the weekend.

Caching is implemented using `IMemoryCache` to store the data and reduce the number of API calls.

## Unit Tests

Unit tests are provided to ensure the functionality of the API. Below are the test cases, grouped by their respective functionalities:

### Currency Exchange Rates

1. **TestCurrencyExchangeRatesSuccess**
    - Verifies that the endpoint returns a 200 status code with correct data.
    
2. **TestCurrencyExchangeRatesSuccessResponse**
    - Verifies that the endpoint returns a 200 status code and the response matches the expected data.
    
3. **TestCurrencyExchangeRatesErrorResponse**
    - Verifies that the endpoint returns a 404 status code when the currency code is invalid.
    
4. **TestCurrencyExchangeRatesExceptionResponse**
    - Verifies that the endpoint returns a 500 status code when an exception occurs.

### Currency Conversion

5. **TestCurrencyConvertSuccess**
    - Verifies that the endpoint returns a 200 status code with correct conversion data.
    
6. **TestCurrencyConvertSuccessResponse**
    - Verifies that the endpoint returns a 200 status code and the response matches the expected data.
    
7. **TestCurrencyConvertExcludedResponse**
    - Verifies that the endpoint returns a 400 status code when the currency conversion is not allowed.
    
8. **TestCurrencyConvertErrorResponse**
    - Verifies that the endpoint returns a 404 status code when the currency conversion is not found.
    
9. **TestCurrencyConvertErrorExceptionResponse**
    - Verifies that the endpoint returns a 500 status code when an exception occurs.

### Historical Rates

10. **TestHistoricalRatesSuccess**
    - Verifies that the endpoint returns a 200 status code with correct historical rates data.
    
11. **TestHistoricalRatesSuccessResponse**
    - Verifies that the endpoint returns a 200 status code and the response matches the expected data.
    
12. **TestHistoricalRatesErrorResponse**
    - Verifies that the endpoint returns a 404 status code when historical rates are not found.
    
13. **TestHistoricalRatesExceptionResponse**
    - Verifies that the endpoint returns a 500 status code when an exception occurs.


### Running Tests in Visual Studio

To run the tests manually using Visual Studio, follow these steps:

1. **Open Solution**:
    - Open the solution containing the unit tests in Visual Studio.

2. **Build Solution**:
    - Build the solution to ensure all dependencies are resolved and there are no compilation errors.
    - Go to the menu and select `Build` > `Build Solution`.

3. **Open Test Explorer**:
    - Go to the menu and select `Test` > `Test Explorer` to open the Test Explorer window.

4. **Run All Tests**:
    - In the Test Explorer window, you will see a list of all available tests.
    - Click on `Run All` to execute all tests.
    - Alternatively, you can right-click on individual tests or test classes and select `Run` to execute specific tests.

5. **View Test Results**:
    - Once the tests are executed, the results will be displayed in the Test Explorer window.
    - You can view details of passed, failed, and skipped tests.

Ensure that you have all dependencies installed and your environment is properly configured.



## Postman Collection

A Postman collection is provided for testing the API endpoints.

**Collection Details:**
- `Currency Exchange Rate`: Fetches the current exchange rates for USD.
- `Currency Convert`: Converts 123 AUD to JPY.
- `Currency Historical Rates`: Fetches historical rates for AUD from 2020-01-14 to 2020-02-06.

**Variables:**
- `urlTest`: Base URL for the external API (`https://api.frankfurter.app`).
- `url`: Base URL for the local API (`https://localhost:7034`).

## Example Usage

## Example Usage

**Get Current Exchange Rates (Default: EUR):**
GET /api/Currency/ExchangeRates

**Get Current Exchange Rates:**
GET /api/Currency/ExchangeRates?currencyCode=USD

**Convert Currency:**
GET /api/Currency/CurrencyConvert?amount=123&from=AUD&to=JPY


**Get Historical Rates:**
GET /api/Currency/HistoricalRates?startDate=2020-01-14&endDate=2020-02-06&currencyCode=AUD&page=1&pageSize=5


## How to Run

1. Clone the repository.
2. Build the project using your preferred IDE.
3. Run the application.
4. Use Postman to test the endpoints using the provided collection.

This documentation provides a comprehensive overview of the Currency API, including endpoint details, caching, unit tests, and Postman collection for testing.


