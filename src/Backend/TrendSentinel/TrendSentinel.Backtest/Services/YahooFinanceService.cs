using System.Text.Json;

namespace TrendSentinel.Backtest.Services
{
    public class YahooFinanceService
    {
        private readonly HttpClient _httpClient;

        public YahooFinanceService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
            _httpClient.Timeout = TimeSpan.FromSeconds(20);
        }

        public async Task<decimal> GetCurrentPriceAsync(string ticker)
        {
            // Önce query1, olmassa query2
            foreach (var host in new[] { "query1", "query2" })
            {
                var price = await FetchCurrentPrice(ticker, host);
                if (price > 0)
                {
                    Console.WriteLine($"    💰 {ticker}: {price:F4} ({host})");
                    return price;
                }
            }

            Console.WriteLine($"    ❌ {ticker}: Güncel fiyat alınamadı.");
            return 0;
        }

        private async Task<decimal> FetchCurrentPrice(string ticker, string host)
        {
            try
            {
                // includePrePost=true → piyasa kapalıyken after-hours fiyatı da gelir
                var url = $"https://{host}.finance.yahoo.com/v8/finance/chart/{ticker}" +
                          $"?interval=1m&range=1d&includePrePost=true";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return 0;

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                var chart = doc.RootElement.GetProperty("chart");
                if (chart.TryGetProperty("error", out var err) && err.ValueKind != JsonValueKind.Null)
                    return 0;

                var result = chart.GetProperty("result")[0];
                var meta = result.GetProperty("meta");

                //  regularMarketPrice — piyasa açıkken anlık, kapalıyken son resmi kapanış
                if (meta.TryGetProperty("regularMarketPrice", out var rmp)
                    && rmp.ValueKind == JsonValueKind.Number
                    && rmp.TryGetDecimal(out var regularPrice)
                    && regularPrice > 0)
                {
                    // Piyasa şu an açık mı kontrol et
                    var marketState = GetStr(meta, "marketState"); // REGULAR, PRE, POST, CLOSED
                    Console.WriteLine($"    ℹ️  {ticker} marketState: {marketState}");

                    if (marketState == "REGULAR")
                    {
                        // Piyasa açık → regularMarketPrice gerçek zamanlı
                        return regularPrice;
                    }

                    // Piyasa kapalı → after-hours/pre-market için son bar'dan al
                    var lastBar = GetLastCloseFromBars(result);
                    if (lastBar > 0 && Math.Abs(lastBar - regularPrice) / regularPrice > 0.001m)
                    {
                        // Son bar regularMarketPrice'tan farklıysa → after-hours hareketi var
                        Console.WriteLine($"    ℹ️  {ticker} after-hours/pre-market: {lastBar:F4} (resmi kapanış: {regularPrice:F4})");
                        return lastBar;
                    }

                    // Fark yoksa regularMarketPrice döndür
                    return regularPrice;
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ⚠️ FetchCurrentPrice hatası ({ticker}/{host}): {ex.Message}");
                return 0;
            }
        }

        private static decimal GetLastCloseFromBars(JsonElement result)
        {
            try
            {
                var closes = result
                    .GetProperty("indicators")
                    .GetProperty("quote")[0]
                    .GetProperty("close")
                    .EnumerateArray()
                    .Select(p => p.TryGetDecimal(out var v) ? v : (decimal?)null)
                    .Where(p => p.HasValue && p.Value > 0)
                    .ToList();

                return closes.Any() ? closes.Last()!.Value : 0;
            }
            catch { return 0; }
        }

        private static string GetStr(JsonElement el, string key)
        {
            if (el.TryGetProperty(key, out var val)) return val.ToString();
            return "";
        }

        public async Task<List<decimal>> GetPriceHistoryAsync(string ticker, DateTime date, int daysCount = 5)
        {
            try
            {
                var endDate = date.AddDays(2);
                var startDate = date.AddDays(-daysCount - 7); // hafta sonu boşlukları için geniş aralık

                var period1 = new DateTimeOffset(startDate, TimeSpan.Zero).ToUnixTimeSeconds();
                var period2 = new DateTimeOffset(endDate, TimeSpan.Zero).ToUnixTimeSeconds();

                var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{ticker}" +
                          $"?interval=1d&period1={period1}&period2={period2}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return new List<decimal>();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                var chart = doc.RootElement.GetProperty("chart");
                if (chart.TryGetProperty("error", out var err) && err.ValueKind != JsonValueKind.Null)
                    return new List<decimal>();

                var resultNode = chart.GetProperty("result")[0];

                var timestamps = resultNode
                    .GetProperty("timestamp")
                    .EnumerateArray()
                    .Select(t => t.GetInt64())
                    .ToList();

                var closePrices = resultNode
                    .GetProperty("indicators")
                    .GetProperty("quote")[0]
                    .GetProperty("close")
                    .EnumerateArray()
                    .Select(p => p.TryGetDecimal(out var val) ? val : 0)
                    .ToList();

                var priceByDate = new Dictionary<DateTime, decimal>();
                for (int i = 0; i < timestamps.Count && i < closePrices.Count; i++)
                {
                    if (closePrices[i] > 0)
                    {
                        var day = DateTimeOffset.FromUnixTimeSeconds(timestamps[i]).UtcDateTime.Date;
                        priceByDate[day] = closePrices[i];
                    }
                }

                var tradingDays = priceByDate.Keys
                    .Where(d => d <= date.Date)
                    .OrderBy(d => d)
                    .ToList();

                if (tradingDays.Count == 0) return new List<decimal>();

                var entryDay = tradingDays.Last();

                var relevantDays = tradingDays
                    .Where(d => d <= entryDay)
                    .TakeLast(daysCount + 1)
                    .ToList();

                while (relevantDays.Count < daysCount + 1)
                    relevantDays.Insert(0, DateTime.MinValue);

                return relevantDays
                    .Select(d => d == DateTime.MinValue ? 0 : priceByDate.GetValueOrDefault(d, 0))
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"     Fiyat geçmişi hatası ({ticker}): {ex.Message}");
                return new List<decimal>();
            }
        }
    }
}