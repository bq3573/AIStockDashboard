using RestSharp;
using Newtonsoft.Json.Linq;
using StockInfoApp.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using StockInfoApp.Utilities;
using System.Globalization;
using StockInfoApp.Services;

public class StockService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey = ""; // add your own api keys
    private readonly string openAiApiKey = ""; 
    private readonly string _baseUrl = "https://www.alphavantage.co/query";
    private readonly ArticleExtractor _articleExtractor;
    private readonly FinnHubService _finnHubService;

    public StockService(HttpClient httpClient, FinnHubService finnHubService)
    {
        _httpClient = httpClient;
        _articleExtractor = new ArticleExtractor();
        _finnHubService = finnHubService;
    }

    public async Task<StockData> GetDailyStockDataAsync(string ticker)
    {
        var url = $"{_baseUrl}?function=GLOBAL_QUOTE&symbol={ticker}&apikey={_apiKey}";
        var response = await _httpClient.GetStringAsync(url);
        dynamic data = JsonConvert.DeserializeObject(response);

        var stockData = new StockData
        {
            Symbol = data["Global Quote"]["01. symbol"],
            Price = data["Global Quote"]["05. price"],
            Open = data["Global Quote"]["02. open"],
            High = data["Global Quote"]["03. high"],
            Low = data["Global Quote"]["04. low"],
            Volume = data["Global Quote"]["06. volume"],
            LatestTradingDay = data["Global Quote"]["07. latest trading day"],
            PreviousClose = data["Global Quote"]["08. previous close"],
            Change = data["Global Quote"]["09. change"],
            ChangePercent = data["Global Quote"]["10. change percent"]
        };

        string percentString = stockData.ChangePercent?.Replace("%", "").Trim();
        if (decimal.TryParse(percentString, out decimal changePercent))
        {
            bool negOrPos = changePercent > 0;
            stockData.NegOrPos = negOrPos;
        }
        //else
        //{
        //    // Handle parse error if needed
        //    stockData.NegOrPos = null; // Or false, or throw, depending on your logic
        //}
        return stockData;
    }


    public async Task<string> GetTickerFilters(string filter)
   {
        var url = $"{_baseUrl}?function=SYMBOL_SEARCH&keywords={filter}&apikey={_apiKey}";

        var response = await _httpClient.GetStringAsync(url);
        dynamic data = JsonConvert.DeserializeObject(response);
        var parseData = data["bestMatches"];
        StringBuilder sb = new StringBuilder();
        foreach (var tickerSuggestion in parseData)
        {
            if (tickerSuggestion["4. region"] == "United States")
            {
                var name = tickerSuggestion["2. name"];
                var symbol = tickerSuggestion["1. symbol"];
                sb.Append($"<option value='{symbol}'>{name}: {symbol}</option>");
            }

        }
        return sb.ToString();
    }

    public async Task<List<MarketNews>> GetMarketNewsAsync(string ticker, string sort = "latest")
    {
        // Construct the URL with the ticker symbol
        var url = $"{_baseUrl}?function=NEWS_SENTIMENT&tickers={ticker}&sort={sort}&apikey={_apiKey}";

        // Make the HTTP GET request
        var response = await _httpClient.GetStringAsync(url);

        // Parse the response
        dynamic data = JsonConvert.DeserializeObject(response);

        // Ensure the response contains the expected "feed" data
        if (data["feed"] == null)
        {
            throw new Exception("No news data found for the given ticker.");
        }

        // Map the response to a list of MarketNews objects
        var marketNews = new List<MarketNews>();

        foreach (var article in data["feed"])
        {

            var x = article["time_published"];
            var date = DateFormatter(article["time_published"].Value);
            marketNews.Add(new MarketNews
            {
                Title = article["title"],
                Url = article["url"],
                Source = article["source"],
                PublishedDate = DateFormatter(article["time_published"].Value),
                SentimentScore = article["overall_sentiment_score"],
                SentimentLabel = article["overall_sentiment_label"]
            });
        }
        var unusableNews = marketNews.Where(x => x.Source == "Zacks Commentary").ToList();
        foreach (var article in unusableNews)
        {
            marketNews.Remove(article);
        }

        return marketNews;
    }

    public string DateFormatter(string date)
    {
        DateTime dateTime = DateTime.ParseExact(date, "yyyyMMddTHHmmss", CultureInfo.InvariantCulture);

        // Format the DateTime into a more readable string
        string formattedDate = dateTime.ToString("MMMM dd, yyyy hh:mm:ss tt");

        return formattedDate;
    }

    public async Task<string> GetSummaryFromChatGPTAsync(string url)
    {


        // OpenAI API endpoint
        string openAiApiUrl = "https://api.openai.com/v1/chat/completions";

        // define the request payload
        var payload = new
        {
            model = "gpt-4", // or use "gpt-3.5-turbo"
            messages = new[]
            {
            new
            {
                role = "system",
                content = "you are an highly skilled investor that summarizes recent news articles relating to stocks and market movement."
            },
            new
            {
                role = "user",
                content = $"summarize this article: {url}"
            }
        },
            max_tokens = 300, // limit the length of the summary
            temperature = 0.7 // adjust creativity (0 = more focused, 1 = more creative)
        };




        // Serialize payload to JSON
        var content = new StringContent(
            Newtonsoft.Json.JsonConvert.SerializeObject(payload),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        // Add headers
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {openAiApiKey}");

        // Send POST request to OpenAI
        var response = await _httpClient.PostAsync(openAiApiUrl, content);

        // Ensure the response is successful
        response.EnsureSuccessStatusCode();

        // Parse the response
        var responseString = await response.Content.ReadAsStringAsync();
        dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseString);

        // Extract and return the summary
        return result.choices[0].message.content.ToString();
    }

    public async Task<string> SummarizeArticle(string articleText)
    {
        //WebScraperService scraper = new WebScraperService();
        var chunks = _articleExtractor.SplitTextIntoChunks(articleText);

        var summarizedText = new StringBuilder();

        foreach (var chunk in chunks)
        {
            var response = await OpenAI_API_Request(chunk);
            summarizedText.AppendLine(response); // Add the result to the summary
        }

        return summarizedText.ToString();
    }

    
    public async Task<string> OpenAI_API_Request(string articleChunk)
    {
        var url = "https://api.openai.com/v1/chat/completions"; // Correct endpoint for GPT-3.5-turbo (chat completions)

        // Create the HttpRequestMessage
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(
                JsonConvert.SerializeObject(new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                    new { role = "system", content = "You are a highly skilled investor." },
                    new { role = "user", content = $"Please summarize the following article:\n\n{articleChunk}" }
                    },
                    max_tokens = 200, // Limit the summary length 
                    temperature = 0.7
                }),
                Encoding.UTF8,
                "application/json"
            )
        };

        // Add the Authorization header to the HttpRequestMessage
        requestMessage.Headers.Add("Authorization", $"Bearer {openAiApiKey}");

        // Send the request
        using (var response = await _httpClient.SendAsync(requestMessage))
        {
            var result = await response.Content.ReadAsStringAsync();

            // Parse the response to get the summary
            dynamic responseData = JsonConvert.DeserializeObject(result);
            var x = responseData.choices[0].message.content;
            return responseData.choices[0].message.content;
        }
    }

    public async Task<string> GetStockAnlysis(string ticker)
    {
        //var httpClient = new HttpClient();
        StockData promptInfo = await GetDailyStockDataAsync(ticker);
        
        // OpenAI API endpoint
        string openAiApiUrl = "https://api.openai.com/v1/chat/completions";

        // Replace with your actual API key

        // define the request payload
        var payload = new
        {
            model = "gpt-4-turbo", // or use "gpt-3.5-turbo" // want to try the realtime preview
            messages = new[]
            {
            new
            {
                role = "system",
                content = "you are an highly skilled investor and market analyst that uses current events and market data to predict the short term outlook of stocks."
            },
            new
            {
                role = "user",
                content = $"Here's today's data for {ticker} stock:" +
                $" Open: ${promptInfo.Price}, High: ${promptInfo.High}" +
                $", Low: ${promptInfo.Low}, Close: ${promptInfo.PreviousClose}" +
                $", Volume: {promptInfo.Volume}" +
                $", Price Change %: {promptInfo.ChangePercent}." +
                $" What does this suggest on the current date of {System.DateTime.Now}?"
            }
        },
            max_tokens = 500, // limit the length of the summary
            temperature = 0.2 // adjust creativity (0 = more focused, 1 = more creative)
        };




        // Serialize payload to JSON
        var content = new StringContent(
            Newtonsoft.Json.JsonConvert.SerializeObject(payload),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        // Add headers
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {openAiApiKey}");

        // Send POST request to OpenAI
        var response = await _httpClient.PostAsync(openAiApiUrl, content);

        // Ensure the response is successful
        response.EnsureSuccessStatusCode();

        // Parse the response
        var responseString = await response.Content.ReadAsStringAsync();
        dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseString);

        // Extract and return the summary
        return result.choices[0].message.content.ToString();
    }


    public async Task<string> GetAiOverview(string ticker)
    {
        //var httpClient = new HttpClient();
        //StockData promptInfo = await GetDailyStockDataAsync(ticker);

        // OpenAI API endpoint
        string openAiApiUrl = "https://api.openai.com/v1/chat/completions";

        // Replace with your actual API key

        // define the request payload
        var payload = new
        {
            model = "gpt-4-turbo",
            messages = new[]
        {
        new
        {
            role = "system",
            content = "You are a highly skilled investor and market analyst who uses current events and market data to summarize companies for short-term outlooks."
        },
        new
        {
            role = "user",
            content = $"Give me a brief overview of the company behind the stock ticker {ticker}. " +
                      "Include what the company does, its industry." +
                      "Keep it short, readable, and make sure it's a complete thought."
        }
        },
            max_tokens = 100, // raised from 50 to ensure complete response
            temperature = 0.2
        };





        // Serialize payload to JSON
        var content = new StringContent(
            Newtonsoft.Json.JsonConvert.SerializeObject(payload),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        // Add headers
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {openAiApiKey}");

        // Send POST request to OpenAI
        var response = await _httpClient.PostAsync(openAiApiUrl, content);

        // Ensure the response is successful
        response.EnsureSuccessStatusCode();

        // Parse the response
        var responseString = await response.Content.ReadAsStringAsync();
        dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseString);
        string cont = result["choices"]?[0]?["message"]?["content"]?.ToString();
        // Extract and return the summary
        return cont;
    }


    public async Task<string> GetAiOutlook(string ticker)
    {
        //var httpClient = new HttpClient();
        //StockData promptInfo = await GetDailyStockDataAsync(ticker);

        // OpenAI API endpoint
        string openAiApiUrl = "https://api.openai.com/v1/chat/completions";

        var news = await _finnHubService.GetTickerNewsSummary(ticker);

        var recentNewsBlock = string.Join("\n", news.Headlines.Zip(news.Summaries,
            (headline, summary) => $"- {headline} {(string.IsNullOrWhiteSpace(summary) ? "" : summary)}"));

        var payload = new
        {
            model = "gpt-4-turbo",
            messages = new[]
        {
        new
        {
            role = "system",
            content = "You are a highly skilled investor and market strategist. You analyze companies using their industry positioning, recent price movement, earnings reports, and news sentiment to form realistic short-term stock outlooks. You keep analysis concise and avoid hype."
        },
        new
        {
            role = "user",
            content =
            $@"Provide a short-term stock outlook for {news.Ticker}. 
            Here are recent news headlines and summaries to guide your response:

            {recentNewsBlock}

            Base your analysis only on the information provided above.
            Be concise, readable, and focused on what might drive price movement in the near future.
            Keep it short, readable, and make sure it's a complete thought."

        }
        },
            max_tokens = 250,
            temperature = 0.3
        };





        // Serialize payload to JSON
        var content = new StringContent(
            Newtonsoft.Json.JsonConvert.SerializeObject(payload),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        // Add headers
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {openAiApiKey}");

        // Send POST request to OpenAI
        var response = await _httpClient.PostAsync(openAiApiUrl, content);

        // Ensure the response is successful
        response.EnsureSuccessStatusCode();

        // Parse the response
        var responseString = await response.Content.ReadAsStringAsync();
        dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseString);
        string cont = result["choices"]?[0]?["message"]?["content"]?.ToString();
        // Extract and return the summary
        return cont;
    }
    //QuoteResponse qr = await _finnHubService.GetQuoteAsync(symbol);
    //TickerNewsSummary tns = await _finnHubService.GetTickerNewsSummary(symbol);
    public async Task<string> SuperchargeAiAnalysis(QuoteResponse qr, TickerNewsSummary tns, string ticker)
    {
        string openAiApiUrl = "https://api.openai.com/v1/chat/completions";

        var news = tns;

        var recentNewsBlock = string.Join("\n", news.Headlines.Zip(news.Summaries,
            (headline, summary) => $"- {headline} {(string.IsNullOrWhiteSpace(summary) ? "" : summary)}"));

        var payload = new
        {
            model = "gpt-4-turbo",
            temperature = 0.7,
            messages = new[]
            {
        new
        {
            role = "system",
            content = "You are a seasoned stock analyst with deep experience in short-term trading strategies. You analyze recent news and current market data to form clear, confident opinions. You are decisive, realistic, and avoid hype."
        },
        new
        {
        role = "user",
        content =
        $@"You are given the current stock quote and a list of recent news headlines and summaries. Based on this information, form a strong opinion:

        **Should this stock be bought right now for a short-term gain (next 30 days)? Why or why not?**
        Include a **realistic 30-day price prediction** based on the data.

        Be decisive. If the stock is not a good buy, explain why. If it is a buy, clearly justify it.

        Here is the data:
        - **Current Price:** ${qr.C}
        - **Change:** ${qr.D} ({qr.Dp}%)
        - **Open:** ${qr.O}
        - **High:** ${qr.H}
        - **Low:** ${qr.L}
        - **Previous Close:** ${qr.Pc}
        - **Overall Sentiment:** {(qr.IsPositive ? "Positive" : "Negative")}

        **Recent News. Use this list of recent news headlines and their respective summaries to inform your opinion:**
        {recentNewsBlock}

        Respond in this format:

        Buy Recommendation: [Yes/No]  
        Reasoning: 
        [Key factors and justification]  
        30-Day Price Prediction:
        $[predictedPrice]  
        Catalyst(s):
        [e.g., earnings beat, analyst upgrades, sector trends]"
        }
    },
        max_tokens = 500,
    };





        // Serialize payload to JSON
        var content = new StringContent(
            Newtonsoft.Json.JsonConvert.SerializeObject(payload),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        // Add headers
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {openAiApiKey}");

        // Send POST request to OpenAI
        var response = await _httpClient.PostAsync(openAiApiUrl, content);

        // Ensure the response is successful
        response.EnsureSuccessStatusCode();

        // Parse the response
        var responseString = await response.Content.ReadAsStringAsync();
        dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseString);
        string cont = result["choices"]?[0]?["message"]?["content"]?.ToString();
        // Extract and return the summary
        return cont;
    }
}