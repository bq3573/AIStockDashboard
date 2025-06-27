namespace StockInfoApp.Models
{
    public class TickerNewsSummary
    {
        public string Ticker { get; set; }
        public List<string> Headlines { get; set; }
        public List<string> Summaries { get; set; }
    }
}
