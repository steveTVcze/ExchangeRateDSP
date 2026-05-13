using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ExchangeRateDSP.Services;
using ExchangeRateDSP.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ExchangeRateDSP.Pages
{
    public class IndexModel : PageModel
    {
        private readonly CurrencyService _currencyService;
        private readonly AppDbContext _context;

        public string BaseCurrency { get; set; } = "EUR";
        public string StrongestCurrency { get; set; } = string.Empty;
        public string WeakestCurrency { get; set; } = string.Empty;
        public string ChartLabelsJson { get; set; } = "[]";
        public string ChartDataJson { get; set; } = "[]";
        public bool ApiFailed { get; set; } = false;

        public IndexModel(CurrencyService currencyService, AppDbContext dbContext)
        {
            _currencyService = currencyService;
            _context = dbContext;
        }

        public async Task OnGetAsync()
        {
            var settings = await _context.UserSettings.FirstOrDefaultAsync();
            var selectedCurrencies = new List<string> { "CZK", "USD", "GBP" }; // Default

            if (settings != null)
            {
                BaseCurrency = settings.BaseCurrency;
                selectedCurrencies = settings.SelectedCurrencies.Split(',').ToList();
            }

            var symbols = string.Join(",", selectedCurrencies);
            var apiData = await _currencyService.GetLatestRatesAsync(BaseCurrency, symbols);
            if (apiData == null || !apiData.Success)
            {
                ApiFailed = true;
                return;
            }

            var strongest = _currencyService.GetStrongestCurrency(apiData.Rates, selectedCurrencies);
            var weakest = _currencyService.GetWeakestCurrency(apiData.Rates, selectedCurrencies);

            StrongestCurrency = $"{strongest.Key} ({strongest.Value})";
            WeakestCurrency = $"{weakest.Key} ({weakest.Value})";
            BaseCurrency = apiData.Base ?? BaseCurrency;
            var filteredRates = apiData.Rates.Where(r => selectedCurrencies.Contains(r.Key)).ToDictionary(r => r.Key, r => r.Value);

            ChartLabelsJson = JsonSerializer.Serialize(filteredRates.Keys);
            ChartDataJson = JsonSerializer.Serialize(filteredRates.Values);
        }
    }
}
