using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ExchangeRateDSP.Data;
using ExchangeRateDSP.Models;
using Microsoft.EntityFrameworkCore;


namespace ExchangeRateDSP.Pages
{
    public class SettingsModel : PageModel
    {
        private readonly AppDbContext _dbContext;

        [BindProperty]
        public string BaseCurrency { get; set; } = string.Empty;

        [BindProperty]
        public string SelectedCurrencies { get; set; } = string.Empty;

        public bool ShowSuccessMessage { get; set; } = false;

        public SettingsModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task OnGetAsync()
        {
            var settings = await _dbContext.UserSettings.FirstOrDefaultAsync();

            if (settings != null)
            {
                BaseCurrency = settings.BaseCurrency;
                SelectedCurrencies = settings.SelectedCurrencies;
            }
            else
            {
                BaseCurrency = "USD";
                SelectedCurrencies = "CZK,GBP,EUR";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var settings = await _dbContext.UserSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new UserSettings();
                _dbContext.UserSettings.Add(settings);
            }

            settings.BaseCurrency = BaseCurrency.ToUpper();
            settings.SelectedCurrencies = SelectedCurrencies.ToUpper().Replace(" ", "");

            await _dbContext.SaveChangesAsync();

            ShowSuccessMessage = true;
            return Page();
        }
    }
}
