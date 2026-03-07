using System;
using TrendSentinel.Domain.Common;
using TrendSentinel.Domain.ValueObjects;

namespace TrendSentinel.Domain.Entities
{
    public class NewsLog : BaseEntity
    {
        // İlişkiler
        public Guid CompanyId { get; private set; }
        public Company Company { get; private set; } = null!;

        // Temel Haber İçeriği
        public string Title { get; private set; } = string.Empty;
        public string Url { get; private set; } = string.Empty;
        public string Summary { get; private set; } = string.Empty; 
        public DateTime PublishedDate { get; private set; }

        // Value Objects (Owned Types - EF Core)
        public AiAnalysisResult? Analysis { get; private set; }
        public QuantSignalMetrics? QuantMetrics { get; private set; }

        // 1:1 İlişkiler (Ayrı Tablolar)
        public PriceHistory? PriceSnapshot { get; private set; }
        public EventTechnicalSnapshot? TechnicalSnapshot { get; private set; }

        private NewsLog() { }

        public static NewsLog Create(Guid companyId, string title, string url, string summary, DateTime publishedDate)
        {
            return new NewsLog
            {
                CompanyId = companyId,
                Title = title,
                Url = url,
                Summary = summary,
                PublishedDate = publishedDate
            };
        }

        public NewsLog UpdateWithAiAnalysis(AiAnalysisResult analysis, QuantSignalMetrics quantMetrics)
        {
            Analysis = analysis;
            QuantMetrics = quantMetrics;
            UpdatedDate = DateTime.UtcNow;
            return this;
        }

        public bool ShouldTriggerAlert()
        {
            return Analysis?.IsTrendTriggered == true &&
                   Analysis?.IsHighConfidence(75) == true;
        }
    }
}