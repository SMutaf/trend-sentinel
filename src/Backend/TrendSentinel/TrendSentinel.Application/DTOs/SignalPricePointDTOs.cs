using System;

namespace TrendSentinel.Application.DTOs
{
    // === CREATE REQUEST ===
    public class CreateSignalPricePointRequest
    {
        public Guid SignalTrackId { get; set; }
        public DateTime Date { get; set; }
        public int DayNumber { get; set; }

        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
    }

    // === RESPONSE ===
    public class SignalPricePointResponse
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public int DayNumber { get; set; }

        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }

        public decimal CumulativeChangePercent { get; set; }
    }

    // === GRAFİK İÇİN OPTİMİZE (Frontend chart) ===
    public class SignalChartPoint
    {
        public int Day { get; set; }           // 0, 1, 2... 10
        public decimal Price { get; set; }     // Close fiyatı
        public decimal Change { get; set; }    // Entry'e göre % değişim
        public DateTime Date { get; set; }     // Gerçek tarih (tooltip için)
    }
}