namespace StockInfoApp.Models
{
    public class TickerQuote
    {
        public string Ticker { get; set; }
        public decimal dp { get; set; } // change %
        public decimal TrendScore { get; set; } // calculated trendiness
    }
}
