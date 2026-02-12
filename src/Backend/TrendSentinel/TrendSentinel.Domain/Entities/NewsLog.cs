using System;
using TrendSentinel.Domain.Common;

namespace TrendSentinel.Domain.Entities
{
    public class NewsLog : BaseEntity
    {
        // Hangi Şirkete Ait?
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        // Haber Detayları
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;

        // --- YAPAY ZEKA ANALİZ SONUÇLARI (V2) ---

        // AI "Burada önemli bir trend var!" dedi mi? (True/False)
        public bool IsTrendTriggered { get; set; }

        // AI'ın yorumu: "Yatırımcı ilgisi artıyor çünkü..."
        public string TrendSummary { get; set; } = string.Empty;

        // Duygu Durumu: Positive / Negative / Neutral
        public string SentimentLabel { get; set; } = string.Empty;

        // Haberin Yayınlanma Tarihi
        public DateTime PublishedDate { get; set; }
    }
}