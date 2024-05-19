namespace CurrencyAPI.Models
{
    public class CurrencyRates
    {
        public decimal? Amount { get; set; }
        public string? Base { get; set; }
        public Dictionary<string, decimal>? Rates { get; set; }
    }
}
