using System.Globalization;
using System.Text;
using TrendSentinel.Backtest.Models;

namespace TrendSentinel.Backtest.Services
{
    public class CsvExportService
    {
        private readonly string _outputPath;
        private static readonly CultureInfo TrCulture = new CultureInfo("tr-TR");

        public CsvExportService(string outputPath)
        {
            _outputPath = outputPath;
            if (!Directory.Exists(_outputPath))
            {
                Directory.CreateDirectory(_outputPath);
                Console.WriteLine($" Output klasörü oluşturuldu: {_outputPath}");
            }
        }

        public string ExportToCsv(List<SignalResult> results)
        {
            if (results == null || results.Count == 0)
            {
                Console.WriteLine(" Export edilecek veri yok.");
                return string.Empty;
            }

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
            var fileName = $"signal_performance_{timestamp}.csv";
            var filePath = Path.Combine(_outputPath, fileName);

            var sb = new StringBuilder();
            sb.AppendLine("Ticker;ExpectedDirection;Price5DaysBefore;Price4DaysBefore;Price3DaysBefore;Price2DaysBefore;Price1DayBefore;EntryPrice;CurrentPrice;ReturnPercent;Result");

            foreach (var result in results)
            {
                var line = string.Join(";",
                    EscapeCsv(result.TickerSymbol),
                    EscapeCsv(result.ExpectedDirection),
                    Fmt(result.Price5DaysBefore),
                    Fmt(result.Price4DaysBefore),
                    Fmt(result.Price3DaysBefore),
                    Fmt(result.Price2DaysBefore),
                    Fmt(result.Price1DayBefore),
                    Fmt(result.EntryPrice),
                    Fmt(result.CurrentPrice),
                    Fmt(result.ReturnPercent),
                    EscapeCsv(result.Result)
                );
                sb.AppendLine(line);
            }

            File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
            Console.WriteLine($" CSV: {filePath} ({results.Count} sinyal)");
            return filePath;
        }

        // Türkçe locale: "15,82" → Excel sayı olarak okur ✅
        // InvariantCulture: "15.82" → Excel Türkçe'de tarih sanır ❌
        private static string Fmt(decimal value) => value.ToString("F2", TrCulture);

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (value.Contains(";") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }

        public void ExportSummary(List<SignalResult> results)
        {
            if (results == null || results.Count == 0) return;

            var wins = results.Count(r => r.Result == "WIN");
            var losses = results.Count(r => r.Result == "LOSS");
            var neutral = results.Count(r => r.Result == "NEUTRAL");
            var total = results.Count;
            var winRate = (double)wins / total * 100;
            var avgReturn = results.Average(r => r.ReturnPercent);

            var summaryPath = Path.Combine(_outputPath, $"summary_{DateTime.Now:yyyy-MM-dd_HH-mm}.txt");
            var sb = new StringBuilder();

            sb.AppendLine("           TREND SENTINEL BACKTEST ÖZETİ               ");
            sb.AppendLine($"Toplam : {total}");
            sb.AppendLine($"WIN    : {wins} ({winRate:F1}%)");
            sb.AppendLine($"LOSS   : {losses}");
            sb.AppendLine($"NEUTRAL: {neutral}");
            sb.AppendLine($"Ort. Getiri: {avgReturn.ToString("F2", TrCulture)}%");
            sb.AppendLine($"Tarih : {DateTime.Now:dd.MM.yyyy HH:mm}");

            File.WriteAllText(summaryPath, sb.ToString(), Encoding.UTF8);
            Console.WriteLine(sb.ToString());
        }
    }
}