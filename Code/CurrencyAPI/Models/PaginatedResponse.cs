namespace CurrencyAPI.Models
{
    public class PaginatedResponse<T>
    {
        public string? Base { get; set; }
        public string? Start_Date { get; set; }
        public string? End_Date { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public Dictionary<string, T>? Rates { get; set; }
    }
}
