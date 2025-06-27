namespace StockInfoApp.Models
{
    public class QuoteResponse
    {
        public decimal C { get; set; }  // Current price
        public decimal D { get; set; }  // Change
        public decimal Dp { get; set; } // Percent change
        public decimal H { get; set; }  // High
        public decimal L { get; set; }  // Low
        public decimal O { get; set; }  // Open
        public decimal Pc { get; set; } // Previous close

        public bool IsPositive { get; set; }
    }
}
