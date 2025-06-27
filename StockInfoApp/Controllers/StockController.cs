using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using StockInfoApp;
using System.Text;
using StockInfoApp.Utilities;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using StockInfoApp.Models;
using System.Net.Http;
using StockInfoApp.Services;

[ApiController]
[Route("api/[controller]")]
public class StockController : Controller
{
    private readonly StockService _stockService;
    private readonly FinnHubService _finnHubService;
    private readonly ArticleExtractor _articleExtractor;
    //private readonly WebScraperService _webScraperService;
    public StockController(StockService stockService, FinnHubService finnHubService)
    {
        //_webScraperService = webScraperService;
        _stockService = stockService; // The DI container will provide this service
        _finnHubService = finnHubService;
        _articleExtractor = new ArticleExtractor();
    }

    [HttpGet("search")]
    public async Task<IActionResult> GetDailyStockData([FromQuery] string ticker)
    {
        try
        {
            var data = await _stockService.GetDailyStockDataAsync(ticker);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }


    [HttpGet("GetTickerFilters")]
    public async Task<IActionResult> GetTickerFilters([FromQuery] string filter)
    {
        try
        {
            var data = await _stockService.GetTickerFilters(filter);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }


    [HttpGet("news")]
    public async Task<IActionResult> GetMarketData([FromQuery] string ticker)
    {
        try
        {
            var data = await _stockService.GetMarketNewsAsync(ticker);
            

            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }


    [HttpGet("summary")]
    public async Task<IActionResult> GetArticleOverview([FromQuery] string url)
    {
        var text = _articleExtractor.ExtractArticleText(url);

        try
        {
            //var data = await _stockService.GetSummaryFromChatGPTAsync(url);
            //return Ok(data);

            var data = await _stockService.SummarizeArticle(text);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("analyze")]
    public async Task<IActionResult> GetStockAnalysis([FromQuery] string ticker)
    {
        try
        {
            var data = await _stockService.GetStockAnlysis(ticker);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }


    [HttpGet("Recommendation")]
    public async Task<IActionResult> GetStockRecommendation([FromQuery] string ticker)
    {
        try
        {
            // Testing a comment
            var data = await _finnHubService.GetRecommendationTrendsAsync(ticker);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }


    [HttpGet("Related")]
    public async Task<IActionResult> GetRelatedTickers([FromQuery] string ticker)
    {
        try
        {
            // Testing a comment
            var data = await _finnHubService.GetRelatedTickers(ticker);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }


    [HttpGet("Overview")]
    public async Task<IActionResult> GetStockOverview([FromQuery] string ticker)
    {
        try
        {
            // Testing a comment
            var data = await _stockService.GetAiOverview(ticker);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }


    [HttpGet("Outlook")]
    public async Task<IActionResult> GetStockOutlook([FromQuery] string ticker)
    {
        try
        {
            // Testing a comment
            var data = await _stockService.GetAiOutlook(ticker);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("Trending")]
    public async Task<IActionResult> GetTrendingStocks()
    {
        try
        {
            var data = await _finnHubService.GetTrendingTickersFromNews();
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("MarketStatus")]
    public async Task<IActionResult> GetMarketStatus()
    {
        try
        {
            var data = await _finnHubService.GetMarketStatus();
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("Quote")]
    public async Task<IActionResult> GetQuote(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            return BadRequest("Symbol is required.");

        try
        {
            var quote = await _finnHubService.GetQuoteAsync(symbol);
            return Ok(quote);
        }
        catch (Exception ex)
        {
            // log exception
            return StatusCode(500, "Failed to fetch quote data.");
        }
    }

    [HttpGet("Supercharge")]
    public async Task<IActionResult> getSuperChrageAnalysis(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            return BadRequest("Symbol is required.");

        


        try
        {
            QuoteResponse qr = await _finnHubService.GetQuoteAsync(symbol);
            TickerNewsSummary tns = await _finnHubService.GetTickerNewsSummary(symbol);
            var data =  await _stockService.SuperchargeAiAnalysis(qr, tns, symbol);
            return Ok(data);
        }
        catch (Exception ex)
        {
            // log exception
            return StatusCode(500, "Failed to fetch quote data.");
        }
    }
}