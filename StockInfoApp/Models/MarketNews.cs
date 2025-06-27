namespace StockInfoApp.Models
{
    public class MarketNews
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Source { get; set; }
        public string PublishedDate { get; set; }
        public decimal SentimentScore { get; set; }
        public string SentimentLabel { get; set; }
    }
}
