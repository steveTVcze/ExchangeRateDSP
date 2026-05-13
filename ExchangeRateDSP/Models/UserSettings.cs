namespace ExchangeRateDSP.Models
{
    public class UserSettings
    {
        public Guid Id { get; set; } //primary key 
        public string BaseCurrency { get; set; } = "EUR";
        public string SelectedCurrencies { get; set; } = "CZK,USD,GBP";
    }
}
