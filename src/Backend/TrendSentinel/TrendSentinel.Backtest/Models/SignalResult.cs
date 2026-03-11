namespace TrendSentinel.Backtest.Models
{
    public class SignalResult
    {
        // Temel Bilgiler
        public Guid NewsLogId { get; set; }
        public string TickerSymbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateTime SignalDate { get; set; }
        public DateTime CreatedDate { get; set; }

        // Sinyal Bilgileri
        public string ExpectedDirection { get; set; } = string.Empty;
        public int ImpactStrength { get; set; }
        public int ConfidenceScore { get; set; }
        public string EventType { get; set; } = string.Empty;
        public bool IsTrendTriggered { get; set; }

        // Fiyat Bilgileri (Son 5 gün + Haber günü)
        public decimal Price5DaysBefore { get; set; }  // T-5
        public decimal Price4DaysBefore { get; set; }  // T-4
        public decimal Price3DaysBefore { get; set; }  // T-3
        public decimal Price2DaysBefore { get; set; }  // T-2
        public decimal Price1DayBefore { get; set; }   // T-1
        public decimal EntryPrice { get; set; }         // T (Haber günü)
        public decimal CurrentPrice { get; set; }       // Şu an

        // Performans Metrikleri
        public decimal ReturnPercent { get; set; }
        public int DaysHeld { get; set; }
        public string Result { get; set; } = string.Empty; // WIN/LOSS/NEUTRAL

        // Ek Bilgiler
        public string TrendSummary { get; set; } = string.Empty;
        public string SentimentLabel { get; set; } = string.Empty;
    }
}