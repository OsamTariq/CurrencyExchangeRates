﻿namespace CurrencyAPI.Models
{
    public class HistoricalRates
    {
        public decimal? Amount { get; set; }
        public string? Base { get; set; }
        public string? Start_Date { get; set; }
        public string? End_Date { get; set; }
        public Dictionary<string, Dictionary<string, decimal>>? Rates { get; set; }
    }
}