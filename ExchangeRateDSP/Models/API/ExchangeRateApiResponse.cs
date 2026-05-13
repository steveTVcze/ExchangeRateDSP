using System.Text.Json.Serialization;

namespace ExchangeRateDSP.Models.API
{
    public class ExchangeRateApiResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("source")]
        public string Base { get; set; } = string.Empty;

        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;

        [JsonPropertyName("quotes")]
        public Dictionary<string, decimal> Rates { get; set; } = new();
    }
}

