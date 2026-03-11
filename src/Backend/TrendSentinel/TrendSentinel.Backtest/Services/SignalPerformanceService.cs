using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TrendSentinel.Backtest.Models;
using TrendSentinel.Domain.Entities;
using TrendSentinel.Infrastructure.Persistence;

namespace TrendSentinel.Backtest.Services
{
    public class SignalPerformanceService
    {
        private readonly TrendSentinelDbContext _dbContext;
        private readonly YahooFinanceService _yahooService;
        private readonly int _daysBeforeNews;
        private readonly bool _includeOnlyTrendTriggered;

        public SignalPerformanceService(
            TrendSentinelDbContext dbContext,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _yahooService = new YahooFinanceService();
            _daysBeforeNews = configuration.GetValue<int>("BacktestSettings:DaysBeforeNews", 5);
            _includeOnlyTrendTriggered = configuration.GetValue<bool>("BacktestSettings:IncludeOnlyTrendTriggered", true);
        }

        public async Task<List<SignalResult>> AnalyzeAllSignalsAsync()
        {
            Console.WriteLine("📦 Veritabanından sinyaller çekiliyor...");

            var query = _dbContext.NewsLogs
                .Include(n => n.Company)
                .Include(n => n.PriceSnapshot)
                .AsQueryable();

            if (_includeOnlyTrendTriggered)
            {
                query = query.Where(n => n.Analysis != null && n.Analysis.IsTrendTriggered);
            }

            var newsLogs = await query.ToListAsync();

            Console.WriteLine($"✅ {newsLogs.Count} sinyal bulundu.");

            var results = new List<SignalResult>();

            foreach (var log in newsLogs)
            {
                try
                {
                    var result = await AnalyzeSingleSignalAsync(log);
                    if (result != null)
                    {
                        results.Add(result);
                        Console.WriteLine($"  ✅ {log.Company.TickerSymbol}: {result.Result} ({result.ReturnPercent:F2}%) | Entry: {result.EntryPrice:F2} | Current: {result.CurrentPrice:F2}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ❌ {log.Company.TickerSymbol} hata: {ex.Message}");
                }

                await Task.Delay(600); // Rate limit - biraz artırıldı (v7 + v8 çift istek için)
            }

            return results;
        }

        private async Task<SignalResult?> AnalyzeSingleSignalAsync(NewsLog log)
        {
            if (log.Company == null || string.IsNullOrEmpty(log.Company.TickerSymbol))
                return null;

            var ticker = log.Company.TickerSymbol;
            var result = new SignalResult
            {
                NewsLogId = log.Id,
                TickerSymbol = ticker,
                CompanyName = log.Company.Name,
                SignalDate = log.PublishedDate,
                CreatedDate = log.CreatedDate,
                ExpectedDirection = log.QuantMetrics?.ExpectedDirection.ToString() ?? "Uncertain",
                ImpactStrength = log.QuantMetrics?.ImpactStrength ?? 0,
                ConfidenceScore = log.Analysis?.ConfidenceScore ?? 0,
                EventType = log.QuantMetrics?.EventType.ToString() ?? "Other",
                IsTrendTriggered = log.Analysis?.IsTrendTriggered ?? false,
                TrendSummary = log.Analysis?.TrendSummary ?? string.Empty,
                SentimentLabel = log.Analysis?.Sentiment.ToString() ?? "Neutral"
            };

            // Fiyat geçmişini al (T-5, T-4, T-3, T-2, T-1, T)
            var priceHistory = await _yahooService.GetPriceHistoryAsync(ticker, log.PublishedDate, 5);

            if (priceHistory.Count >= 6)
            {
                result.Price5DaysBefore = priceHistory[0];
                result.Price4DaysBefore = priceHistory[1];
                result.Price3DaysBefore = priceHistory[2];
                result.Price2DaysBefore = priceHistory[3];
                result.Price1DayBefore = priceHistory[4];
                result.EntryPrice = priceHistory[5]; // Haber günü (T)
            }
            else if (log.PriceSnapshot != null)
            {
                // Fiyat geçmişi alınamazsa veritabanındaki snapshot'ı kullan
                result.EntryPrice = log.PriceSnapshot.Close;
                Console.WriteLine($"    ℹ️ {ticker}: Yahoo'dan geçmiş fiyat alınamadı, DB snapshot kullanıldı ({result.EntryPrice:F2})");
            }

            //  Current Price - gerçek zamanlı fiyat
            result.CurrentPrice = await _yahooService.GetCurrentPriceAsync(ticker);

            //  Current price alınamazsa EntryPrice'a eşitleme - uyarı ver
            if (result.CurrentPrice == 0 && result.EntryPrice > 0)
            {
                Console.WriteLine($"     {ticker}: Güncel fiyat alınamadı! ReturnPercent hesaplanamıyor.");
            }

            // Days Held
            result.DaysHeld = (DateTime.Now - log.CreatedDate).Days;

            // Return Percent
            if (result.EntryPrice > 0 && result.CurrentPrice > 0)
            {
                result.ReturnPercent = (result.CurrentPrice - result.EntryPrice) / result.EntryPrice * 100;
            }
            else
            {
                result.ReturnPercent = 0;
            }

            // Result (WIN/LOSS/NEUTRAL)
            result.Result = CalculateResult(result);

            return result;
        }

        private string CalculateResult(SignalResult result)
        {
            //  CurrentPrice alınamadıysa NEUTRAL döndür (yanlış WIN/LOSS hesabını önle)
            if (result.CurrentPrice == 0)
                return "NEUTRAL";

            if (result.ExpectedDirection == "Up")
            {
                if (result.ReturnPercent > 0) return "WIN";
                if (result.ReturnPercent < -5) return "LOSS";
                return "NEUTRAL";
            }
            else if (result.ExpectedDirection == "Down")
            {
                if (result.ReturnPercent < 0) return "WIN";
                if (result.ReturnPercent > 5) return "LOSS";
                return "NEUTRAL";
            }
            return "NEUTRAL";
        }
    }
}