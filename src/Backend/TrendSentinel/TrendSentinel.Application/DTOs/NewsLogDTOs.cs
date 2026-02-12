using System;

namespace TrendSentinel.Application.DTOs
{
    public class CreateNewsLogRequest
    {
        public Guid CompanyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;

        // V2: AI Trend Analiz Sonuçları
        public bool IsTrendTriggered { get; set; }
        public string TrendSummary { get; set; } = string.Empty;
        public string SentimentLabel { get; set; } = string.Empty;

        public DateTime PublishedDate { get; set; }
    }

    public class NewsLogResponse
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;

        public bool IsTrendTriggered { get; set; }
        public string TrendSummary { get; set; } = string.Empty;
        public string SentimentLabel { get; set; } = string.Empty;

        public DateTime PublishedDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}