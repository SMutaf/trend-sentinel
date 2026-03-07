using System;
using TrendSentinel.Domain.Enums;

namespace TrendSentinel.Application.DTOs
{
    public class CreateNewsLogRequest
    {
        public Guid CompanyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;

        // AI Analiz Alanları
        public bool IsTrendTriggered { get; set; }
        public string TrendSummary { get; set; } = string.Empty;
        public string SentimentLabel { get; set; } = string.Empty;  // String kalacak (mapping'de enum'a çevrilecek)
        public DateTime PublishedDate { get; set; }

        // Quant Alanları - STRING OLARAK KALACAK (Python'dan string geliyor)
        public string EventType { get; set; } = string.Empty;
        public int ImpactStrength { get; set; }
        public string ExpectedDirection { get; set; } = string.Empty;
        public string TimeHorizon { get; set; } = string.Empty;
        public bool OverextendedRisk { get; set; }
        public int ConfidenceScore { get; set; }
        public int SectorId { get; set; }
    }

    public class NewsLogResponse
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string TickerSymbol { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public DateTime CreatedDate { get; set; }

        // AI Analiz Alanları
        public bool IsTrendTriggered { get; set; }
        public string TrendSummary { get; set; } = string.Empty;
        public string SentimentLabel { get; set; } = string.Empty;
        public int ConfidenceScore { get; set; }

        // Quant Alanları
        public string EventType { get; set; } = string.Empty;
        public int ImpactStrength { get; set; }
        public string ExpectedDirection { get; set; } = string.Empty;
        public string TimeHorizon { get; set; } = string.Empty;
        public bool OverextendedRisk { get; set; }
    }
}