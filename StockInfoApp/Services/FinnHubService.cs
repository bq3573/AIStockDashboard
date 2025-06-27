using Microsoft.AspNetCore.Mvc;
using StockInfoApp.Models;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace StockInfoApp.Services
{
    public class FinnHubService
    {
        private readonly HttpClient _httpClient;
        private const string ApiKey = ""; // Replace with your actual API key
        public static readonly List<string> SP500Tickers = new()
        {
            "AAPL", "MSFT", "AMZN", "NVDA", "GOOGL", "META", "BRK.B", "UNH", "LLY", "TSLA",
            "JPM", "V", "JNJ", "XOM", "PG", "MA", "AVGO", "HD", "COST", "MRK",
            "PEP", "ABBV", "ADBE", "KO", "CRM", "WMT", "BAC", "PFE", "CVX", "NFLX",
            "TMO", "DIS", "ABT", "CSCO", "LIN", "ACN", "MCD", "DHR", "INTC", "VZ",
            "TXN", "NEE", "WFC", "NKE", "AMD", "PM", "AMGN", "UNP", "MS", "LOW",
            "BMY", "INTU", "HON", "GS", "CAT", "SPGI", "QCOM", "ISRG", "DE", "PLD",
            "RTX", "NOW", "LMT", "AMAT", "IBM", "T", "BLK", "GILD", "ADI", "ZTS",
            "MDT", "BKNG", "GE", "CB", "SYK", "MDLZ", "C", "ADP", "VRTX", "REGN",
            "MO", "CI", "SCHW", "PANW", "ELV", "USB", "MMC", "ADSK", "CL", "LRCX",
            "PGR", "ETN", "TJX", "CDNS", "AXP", "NSC", "SO", "HUM", "BDX", "AON",
            "DUK", "CSX", "SHW", "EQIX", "FDX", "COF", "WM", "FISV", "APD", "EMR",
            "ILMN", "PSX", "ORLY", "ITW", "MRNA", "ROP", "AIG", "EOG", "MCK", "ATVI",
            "MAR", "KLAC", "TRV", "HCA", "CMG", "PCAR", "ECL", "IDXX", "CTAS", "AEP",
            "D", "STZ", "SRE", "OXY", "KMB", "EXC", "NOC", "MCO", "WMB", "FTNT",
            "PRU", "F", "HPQ", "VLO", "PAYX", "HLT", "WELL", "ANET", "ED", "DLR",
            "CMCSA", "TFC", "GIS", "DOW", "ROST", "CME", "MTD", "BKR", "HAL", "EBAY",
            "KR", "ALL", "EA", "KEYS", "XEL", "PSA", "TT", "YUM", "CTSH", "MSI",
            "BIIB", "TEL", "TSCO", "A", "ADM", "BAX", "PEG", "AMP", "AFL", "PPG",
            "DXCM", "CTRA", "LEN", "DVN", "GEN", "RSG", "DLTR", "CNC", "WBA", "RMD",
            "AVB", "HIG", "ATO", "WTW", "PPL", "FITB", "VTR", "NTRS", "HSY", "CHD",
            "CF", "GLW", "NDAQ", "FE", "HBAN", "IFF", "VFC", "ZBH", "INCY", "EXR",
            "NI", "LW", "ALB", "LH", "DHI", "MTB", "DRI", "FMC", "SIVB", "AKAM", "LUV",
            "WHR", "LNT", "BXP", "MKTX", "NRG", "FOX", "FOXA", "TPR", "JBHT", "GNRC",
            "MAS", "HOLX", "UHS", "TECH", "HPE", "IP", "APA", "QRVO", "BR", "SEE",
            "ZION", "NWL", "CAG", "MOS", "ALK", "PNR", "NCLH", "AAL", "DXC", "RL"
        };



        public FinnHubService(HttpClient httpClient)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://finnhub.io/api/v1/")
            };
        }

        public async Task<List<RecommendationTrend>> GetRecommendationTrendsAsync(string symbol)
        {
            var response = await _httpClient.GetAsync($"stock/recommendation?symbol={symbol}&token={ApiKey}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<RecommendationTrend>>(json);
        }

        public async Task<List<string>> GetRelatedTickers(string ticker)
        {
            var response = await _httpClient.GetAsync($"stock/peers?symbol={ticker}&token={ApiKey}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var tickers = JsonSerializer.Deserialize<List<string>>(json);

            return tickers ?? new List<string>();
        }

        public async Task<List<TickerQuote>> GetTrendingTickersFromNews()
        {
            string url = $"https://finnhub.io/api/v1/news?category=general&token={ApiKey}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var newsItems = JsonSerializer.Deserialize<List<FinnhubNewsItem>>(json);

            var tickerCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in newsItems)
            {
                string text = (item.headline + " " + item.summary).ToUpper();

                foreach (var ticker in GetSMP500Tickers())
                {
                    if (text.Contains(ticker))
                    {
                        if (!tickerCounts.ContainsKey(ticker))
                            tickerCounts[ticker] = 0;

                        tickerCounts[ticker]++;
                    }
                }
            }

            var quotes = new List<TickerQuote>();
            var topTenMentions = tickerCounts.OrderByDescending(x => x.Value);
            foreach (var kvp in topTenMentions.Take(16))
            {
                string ticker = kvp.Key;
                int mentions = kvp.Value;

                try
                {
                    string quoteUrl = $"https://finnhub.io/api/v1/quote?symbol={ticker}&token={ApiKey}";
                    var quoteJson = await _httpClient.GetStringAsync(quoteUrl);

                    var quoteData = JsonSerializer.Deserialize<JsonElement>(quoteJson);
                    if (quoteData.TryGetProperty("dp", out var dpElement))
                    {
                        decimal dp = dpElement.GetDecimal();
                        if (dp != 0)
                        {
                            quotes.Add(new TickerQuote
                            {
                                Ticker = ticker,
                                dp = dp,
                                TrendScore = mentions * Math.Abs(dp)
                            });
                        }
                    }
                }
                catch
                {
                    // skip bad data
                }
            }

            return quotes
                .OrderByDescending(q => q.TrendScore)
                .Take(16)
                .ToList();
        }

        public async Task<TickerNewsSummary> GetTickerNewsSummary(string ticker)
        {
            var fromDate = DateTime.UtcNow.AddDays(-10).ToString("yyyy-MM-dd");
            var toDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var url = $"https://finnhub.io/api/v1/company-news?symbol={ticker}&from={fromDate}&to={toDate}&token={ApiKey}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var newsItems = JsonSerializer.Deserialize<List<FinnhubNewsItem>>(json);

            if (newsItems == null || newsItems.Count == 0)
            {
                return new TickerNewsSummary
                {
                    Ticker = ticker,
                    Headlines = new List<string>(),
                    Summaries = new List<string>()
                };
            }

            var topNews = newsItems
                .Where(n => !string.IsNullOrWhiteSpace(n.headline))
                .Take(10)
                .ToList();

            return new TickerNewsSummary
            {
                Ticker = ticker,
                Headlines = topNews.Select(n => n.headline).ToList(),
                Summaries = topNews.Select(n => n.summary).ToList()
            };
        }

        public async Task<MarketStatusResponse> GetMarketStatus()
        {
            var market = "US";
            var url = $"stock/market-status?exchange={market}&token={ApiKey}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to fetch market status: {response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var status = JsonSerializer.Deserialize<MarketStatusResponse>(json, options);

            if (status == null)
                throw new Exception("Error parsing market status response.");

            return status;
        }

        public async Task<QuoteResponse> GetQuoteAsync(string symbol)
        {

            var response = await _httpClient.GetAsync($"quote?symbol={symbol}&token={ApiKey}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<QuoteResponse>(json, options);


            bool negOrPos = data.Pc > 0;
            data.IsPositive = negOrPos;
            

            return data;
        }


        public List<string> GetSMP500Tickers()
        {
                return new()
            {
                "AAPL", "MSFT", "AMZN", "NVDA", "GOOGL", "META", "BRK.B", "UNH", "LLY", "TSLA",
                "JPM", "V", "JNJ", "XOM", "PG", "MA", "AVGO", "HD", "COST", "MRK",
                "PEP", "ABBV", "ADBE", "KO", "CRM", "WMT", "BAC", "PFE", "CVX", "NFLX",
                "TMO", "DIS", "ABT", "CSCO", "LIN", "ACN", "MCD", "DHR", "INTC", "VZ",
                "TXN", "NEE", "WFC", "NKE", "AMD", "PM", "AMGN", "UNP", "MS", "LOW",
                "BMY", "INTU", "HON", "GS", "CAT", "SPGI", "QCOM", "ISRG", "DE", "PLD",
                "RTX", "NOW", "LMT", "AMAT", "IBM", "T", "BLK", "GILD", "ADI", "ZTS",
                "MDT", "BKNG", "GE", "CB", "SYK", "MDLZ", "C", "ADP", "VRTX", "REGN",
                "MO", "CI", "SCHW", "PANW", "ELV", "USB", "MMC", "ADSK", "CL", "LRCX",
                "PGR", "ETN", "TJX", "CDNS", "AXP", "NSC", "SO", "HUM", "BDX", "AON",
                "DUK", "CSX", "SHW", "EQIX", "FDX", "COF", "WM", "FISV", "APD", "EMR",
                "ILMN", "PSX", "ORLY", "ITW", "MRNA", "ROP", "AIG", "EOG", "MCK", "ATVI",
                "MAR", "KLAC", "TRV", "HCA", "CMG", "PCAR", "ECL", "IDXX", "CTAS", "AEP",
                "D", "STZ", "SRE", "OXY", "KMB", "EXC", "NOC", "MCO", "WMB", "FTNT",
                "PRU", "F", "HPQ", "VLO", "PAYX", "HLT", "WELL", "ANET", "ED", "DLR",
                "CMCSA", "TFC", "GIS", "DOW", "ROST", "CME", "MTD", "BKR", "HAL", "EBAY",
                "KR", "ALL", "EA", "KEYS", "XEL", "PSA", "TT", "YUM", "CTSH", "MSI",
                "BIIB", "TEL", "TSCO", "A", "ADM", "BAX", "PEG", "AMP", "AFL", "PPG",
                "DXCM", "CTRA", "LEN", "DVN", "GEN", "RSG", "DLTR", "CNC", "WBA", "RMD",
                "AVB", "HIG", "ATO", "WTW", "PPL", "FITB", "VTR", "NTRS", "HSY", "CHD",
                "CF", "GLW", "NDAQ", "FE", "HBAN", "IFF", "VFC", "ZBH", "INCY", "EXR",
                "NI", "LW", "ALB", "LH", "DHI", "MTB", "DRI", "FMC", "SIVB", "AKAM", "LUV",
                "WHR", "LNT", "BXP", "MKTX", "NRG", "FOX", "FOXA", "TPR", "JBHT", "GNRC",
                "MAS", "HOLX", "UHS", "TECH", "HPE", "IP", "APA", "QRVO", "BR", "SEE",
                "ZION", "NWL", "CAG", "MOS", "ALK", "PNR", "NCLH", "AAL", "DXC", "RL"
            };
        }

    }
}
