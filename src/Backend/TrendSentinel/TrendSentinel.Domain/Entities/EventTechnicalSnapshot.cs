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
    }
}