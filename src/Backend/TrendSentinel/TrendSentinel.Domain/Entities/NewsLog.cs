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

        // --- YAPAY ZEKA TEMEL ANALİZ SONUÇLARI ---

        // AI önemli bir trend haberi buldu mu? (True/False)
        public bool IsTrendTriggered { get; set; }

        // Trend sebebi AI yorumu
        public string TrendSummary { get; set; } = string.Empty;

        // Duygu Durumu: Positive / Negative / Neutral
        public string SentimentLabel { get; set; } = string.Empty;

        // Haberin Yayınlanma Tarihi
        public DateTime PublishedDate { get; set; }

        // --- YENİ EKLENEN QUANT (ALGORİTMİK TRADE) ALANLARI ---

        // Haberin Tipi (Earnings, FDA Approval, Upgrade, Hype vs.)
        public string EventType { get; set; } = string.Empty;

        // Haberin Etki Gücü (1'den 5'e kadar)
        public int ImpactStrength { get; set; }

        // Beklenen Yön (Up, Down, Uncertain)
        public string ExpectedDirection { get; set; } = string.Empty;

        // Etki Süresi (Intraday, ShortTerm, LongTerm)
        public string TimeHorizon { get; set; } = string.Empty;

        // Hisse çok şişmiş mi / düzeltme yeme (mean reversion) riski var mı? (True/False)
        public bool OverextendedRisk { get; set; }

        // Yapay Zekanın Kararına Güven Skoru (0 - 100 arası)
        public int ConfidenceScore { get; set; }
    }
}