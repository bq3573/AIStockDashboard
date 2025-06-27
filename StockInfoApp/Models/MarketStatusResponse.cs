namespace StockInfoApp.Models
{
    public class MarketStatusResponse
    {
        public string Exchange { get; set; }
        public string Holiday { get; set; } // or DateTime? if it's a date
        public bool IsOpen { get; set; }
        public string Session { get; set; }
        public long T { get; set; } // or DateTime if you plan to convert from timestamp
        public string Timezone { get; set; }
    }

}
