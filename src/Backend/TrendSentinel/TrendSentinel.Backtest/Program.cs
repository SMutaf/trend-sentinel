using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TrendSentinel.Backtest.Services;
using TrendSentinel.Infrastructure.Persistence;

namespace TrendSentinel.Backtest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("        TREND SENTINEL BACKTEST SYSTEM v1.0            ");
            Console.WriteLine();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            
            Console.WriteLine();

            try
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                var optionsBuilder = new DbContextOptionsBuilder<TrendSentinelDbContext>();
                optionsBuilder.UseNpgsql(connectionString);

                using var dbContext = new TrendSentinelDbContext(optionsBuilder.Options);

                Console.WriteLine(" Veritabanı bağlantısı kontrol ediliyor...");
                await dbContext.Database.CanConnectAsync();
                Console.WriteLine(" Veritabanı bağlantısı başarılı.");
                Console.WriteLine();

                var performanceService = new SignalPerformanceService(dbContext, configuration);

                var results = await performanceService.AnalyzeAllSignalsAsync();

                Console.WriteLine();
                Console.WriteLine($" Toplam {results.Count} sinyal analiz edildi.");
                Console.WriteLine();

                var csvPath = configuration.GetValue<string>("BacktestSettings:CsvOutputPath", "./output");
                var csvService = new CsvExportService(csvPath);

                csvService.ExportToCsv(results);
                csvService.ExportSummary(results);

                Console.WriteLine();
                Console.WriteLine("                    BACKTEST TAMAMLANDI                ");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine(" HATA OLUŞTU!");
                Console.WriteLine($"   Mesaj: {ex.Message}");
                Console.WriteLine($"   Detay: {ex.InnerException?.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Program sonlandırıldı. Kapatmak için bir tuşa basın...");
            Console.ReadKey();
        }
    }
}