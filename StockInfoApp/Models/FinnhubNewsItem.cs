using System.Text.Json.Serialization;

namespace StockInfoApp.Models
{
    public class FinnhubNewsItem
    {
        public string headline { get; set; }
        public string summary { get; set; }
    }
}
