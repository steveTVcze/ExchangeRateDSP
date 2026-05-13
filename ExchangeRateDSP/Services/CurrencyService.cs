using ExchangeRateDSP.Data;
using ExchangeRateDSP.Models;
using ExchangeRateDSP.Models.API;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ExchangeRateDSP.Services
{
    public class CurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public CurrencyService(HttpClient httpClient, AppDbContext dbContext, IConfiguration config)
        {
            _httpClient = httpClient;
            _context = dbContext;
            _httpClient.BaseAddress = new Uri("https://api.exchangerate.host/");
            _config = config;
        }

        public async Task<ExchangeRateApiResponse?> GetLatestRatesAsync(string baseCurrency, string symbols)
        {
            var apiKey = _config["ExchangeRateApi:ApiKey"];
            var url = $"live?access_key={apiKey}&base={baseCurrency}&symbols={symbols}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                Console.WriteLine("\n--- raw data z apiny ---");
                Console.WriteLine(jsonString);
                var apiData = JsonSerializer.Deserialize<ExchangeRateApiResponse>(jsonString);

                if (apiData != null && apiData.Rates != null)
                {
                    var cleanedRates = new Dictionary<string, decimal>();
                    string baseCurr = apiData.Base ?? "USD";

                    foreach (var kvp in apiData.Rates)
                    {
                        string currencyCode = kvp.Key.StartsWith(baseCurr) ? kvp.Key.Substring(baseCurr.Length) : kvp.Key;

                        cleanedRates[currencyCode] = kvp.Value;
                    }

                    apiData.Rates = cleanedRates;
                }


                return apiData;
            }
            catch (Exception ex)
            {
                var log = new AppLog
                {
                    Level = "Error",
                    Message = $"Chyba při volání API: {ex.Message}",
                    Timestamp = DateTime.UtcNow
                };

                // Uložíme do databáze
                _context.Logs.Add(log);
                await _context.SaveChangesAsync();

                return null;
            }
        }

        // nejsilnejsi mena
        public KeyValuePair<string, decimal> GetStrongestCurrency(Dictionary<string, decimal> rates, List<string> selectedCurrencies)
        {
            if (rates == null || !rates.Any())
            {
                return new KeyValuePair<string, decimal>("Žádná data", 0);
            }
            var filteredRates = rates.Where(r => selectedCurrencies.Contains(r.Key)).ToList();

            if (!filteredRates.Any())
            {
                return new KeyValuePair<string, decimal>("Nenalezeno", 0);
            }
            return filteredRates.OrderByDescending(r => r.Value).First();
        }

        // nejslabsi mena
        public KeyValuePair<string, decimal> GetWeakestCurrency(Dictionary<string, decimal> rates, List<string> selectedCurrencies)
        {
            if (rates == null || !rates.Any())
            {
                return new KeyValuePair<string, decimal>("Žádná data", 0);
            }

            var filteredRates = rates.Where(r => selectedCurrencies.Contains(r.Key)).ToList();

            if (!filteredRates.Any())
            {
                return new KeyValuePair<string, decimal>("Nenalezeno", 0);
            }
            return filteredRates.OrderBy(r => r.Value).First();
        }

        // prumer za obdobi
        public decimal GetAverageRate(List<decimal> historicalRates)
        {
            // kdyz jsou data null vratime 0 - dle zadání: "Chybějící data: ignorovat"
            if (historicalRates == null || !historicalRates.Any())
            {
                return 0;
            }

            return historicalRates.Average();
        }
    }
}
