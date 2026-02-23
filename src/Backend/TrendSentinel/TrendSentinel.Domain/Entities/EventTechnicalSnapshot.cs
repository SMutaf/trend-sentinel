using System;
using TrendSentinel.Domain.Common;

namespace TrendSentinel.Domain.Entities
{
    public class EventTechnicalSnapshot : BaseEntity
    {
        // Bu teknik analiz hangi habere ait?
        public Guid NewsLogId { get; set; }
        public NewsLog NewsLog { get; set; } = null!;

        // Teknik Veriler
        public decimal RsiValue { get; set; }
        public string MacdState { get; set; } = string.Empty;
        public int TechScore { get; set; }
        public bool IsOverextended { get; set; }    

        //Volume teknik verileri
        public decimal VolRatio { get; set; }        // 20 günlük ortalamaya oranı (Örn: 1.5 katı)
        public string VolTrend { get; set; } = string.Empty; // "Strong", "Weak", "Moderate"
        public int AboveAvgDaysLast5 { get; set; }   // Son 5 günde kaç kere ortalamayı geçti?
    }
}