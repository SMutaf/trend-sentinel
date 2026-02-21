using System;
using TrendSentinel.Domain.Common;

namespace TrendSentinel.Domain.Entities
{
    public class PriceHistory : BaseEntity
    {
        // Hangi şirketin fiyatı?
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        // Olay Anı / Mumun Tarihi
        public DateTime Date { get; set; }

        // OHLCV Verileri (Saf Fiyatlar)
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
    }
}