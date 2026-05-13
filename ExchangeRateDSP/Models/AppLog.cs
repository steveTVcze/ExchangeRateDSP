namespace ExchangeRateDSP.Models
{
    public class AppLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Level { get; set; } = "Error";
        public string Message { get; set; } = string.Empty;
    }
}
