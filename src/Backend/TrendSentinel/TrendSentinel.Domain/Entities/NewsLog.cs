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

        // --- YAPAY ZEKA ANALİZ SONUÇLARI ---

        // AI önemli bir trend haberi buldu mu? (True/False)
        public bool IsTrendTriggered { get; set; }

        // Trend sebebi AI yorumu
        public string TrendSummary { get; set; } = string.Empty;

        // Duygu Durumu: Positive / Negative / Neutral
        public string SentimentLabel { get; set; } = string.Empty;

        // Haberin Yayınlanma Tarihi
        public DateTime PublishedDate { get; set; }
    }
}