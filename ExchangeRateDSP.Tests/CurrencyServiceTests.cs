using ExchangeRateDSP.Data;
using ExchangeRateDSP.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ExchangeRateDSP.Tests
{
    public class CurrencyServiceTests
    {
        private readonly CurrencyService _service;

        public CurrencyServiceTests()
        {
            var mockHttp = new Mock<HttpMessageHandler>();
            var client = new HttpClient(mockHttp.Object) { BaseAddress = new Uri("http://localhost") };

            _service = new CurrencyService(client, null, null);
        }

        [Fact]
        public void GetStrongestCurrency_ShouldReturnHighestValue()
        {
            var rates = new Dictionary<string, decimal>
            {
                { "CZK", 22.5m },
                { "EUR", 0.9m },
                { "GBP", 0.7m }
            };
            var selected = new List<string> { "CZK", "EUR", "GBP" };

            var result = _service.GetStrongestCurrency(rates, selected);
            Assert.Equal("CZK", result.Key);
            Assert.Equal(22.5m, result.Value);
        }

        [Fact]
        public void GetStrongestCurrency_WithEmptyRates_ShouldReturnFallback()
        {
            var rates = new Dictionary<string, decimal>();
            var selected = new List<string> { "CZK" };

            var result = _service.GetStrongestCurrency(rates, selected);
            Assert.Equal("Žádná data", result.Key);
            Assert.Equal(0, result.Value);
        }

        [Fact]
        public void GetAverageRate_WithValidData_ShouldCalculateCorrectly()
        {
            var history = new List<decimal> { 20m, 22m, 24m };
            var result = _service.GetAverageRate(history);
            Assert.Equal(22m, result);
        }

        [Fact]
        public void GetWeakestCurrency_ShouldReturnLowestValue()
        {
            var rates = new Dictionary<string, decimal> { { "CZK", 22.5m }, { "EUR", 0.9m }, { "GBP", 0.7m } };
            var selected = new List<string> { "CZK", "EUR", "GBP" };
            var result = _service.GetWeakestCurrency(rates, selected);
            Assert.Equal("GBP", result.Key);
            Assert.Equal(0.7m, result.Value);
        }

        [Fact]
        public void GetWeakestCurrency_WithEmptyRates_ShouldReturnFallback()
        {
            var rates = new Dictionary<string, decimal>();
            var selected = new List<string> { "CZK" };
            var result = _service.GetWeakestCurrency(rates, selected);
            Assert.Equal("Žádná data", result.Key);
        }


        [Fact]
        public void GetStrongestCurrency_WithNoMatchingCurrencies_ShouldReturnNotFound()
        {
            var rates = new Dictionary<string, decimal> { { "USD", 1.0m } };
            var selected = new List<string> { "CZK" };
            var result = _service.GetStrongestCurrency(rates, selected);
            Assert.Equal("Nenalezeno", result.Key);
        }

        [Fact]
        public void GetWeakestCurrency_WithNoMatchingCurrencies_ShouldReturnNotFound()
        {
            var rates = new Dictionary<string, decimal> { { "USD", 1.0m } };
            var selected = new List<string> { "CZK" };
            var result = _service.GetWeakestCurrency(rates, selected);
            Assert.Equal("Nenalezeno", result.Key);
        }

        [Fact]
        public async Task GetLatestRatesAsync_ShouldReturnCleanedData()
        {
            var fakeJson = @"{
                ""success"": true,
                ""source"": ""USD"",
                ""quotes"": {
                    ""USDCZK"": 22.5,
                    ""USDEUR"": 0.9,
                    ""BTC"": 50000.0
                }
            }";
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage{ 
                StatusCode = HttpStatusCode.OK, 
                Content = new StringContent(fakeJson)
            });

            var client = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost") };

            var configMock = new Mock<IConfiguration>();
            
            configMock.Setup(c => c["ExchangeRateApi:ApiKey"]).Returns("FAKE_KEY");

            var service = new CurrencyService(client, null, configMock.Object);
            var result = await service.GetLatestRatesAsync("USD", "CZK,EUR");

            Assert.NotNull(result);
            Assert.True(result.Success);

            Assert.Equal(22.5m, result.Rates["CZK"]);
            Assert.Equal(0.9m, result.Rates["EUR"]);
        }


        [Fact]
        public async Task GetLatestRatesAsync_ApiThrowsException_ShouldLogAndReturnNull()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestErrorDb")
                .Options;
            using var dbContext = new AppDbContext(options);

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Simulovaný výpadek API!"));

            var client = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost") };

            var configMock = new Mock<IConfiguration>();
            var service = new CurrencyService(client, dbContext, configMock.Object);

            var result = await service.GetLatestRatesAsync("USD", "CZK");
            Assert.Null(result);
            Assert.Equal(1, dbContext.Logs.Count());
            Assert.Equal("Error", dbContext.Logs.First().Level);
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public void GetStrongestCurrency_WithNullRates_ShouldReturnFallback()
        {
            var selected = new List<string> { "CZK" };
            var result = _service.GetStrongestCurrency(null, selected);
            Assert.Equal("Žádná data", result.Key);
        }

        [Fact]
        public void GetWeakestCurrency_WithNullRates_ShouldReturnFallback()
        {
            var selected = new List<string> { "CZK" };
            var result = _service.GetWeakestCurrency(null, selected);
            Assert.Equal("Žádná data", result.Key);
        }

        [Fact]
        public void GetAverageRate_WithNullData_ShouldReturnZero()
        {
            var result = _service.GetAverageRate(null);
            Assert.Equal(0m, result);
        }

        [Fact]
        public void GetAverageRate_WithEmptyData_ShouldReturnZero()
        {
            var result = _service.GetAverageRate(new List<decimal>());
            Assert.Equal(0m, result);
        }
    }
}
