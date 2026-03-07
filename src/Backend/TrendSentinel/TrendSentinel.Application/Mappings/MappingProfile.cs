using AutoMapper;
using TrendSentinel.Domain.Entities;
using TrendSentinel.Domain.ValueObjects;
using TrendSentinel.Application.DTOs;
using TrendSentinel.Domain.Enums;

namespace TrendSentinel.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Company Eşleştirmeleri
            CreateMap<Company, CompanyResponse>();
            CreateMap<CreateCompanyRequest, Company>()
                .ForMember(dest => dest.IsAlertSent, opt => opt.MapFrom(src => false));

            // NewsLog -> Response (Flat yapıya dönüştürme)
            CreateMap<NewsLog, NewsLogResponse>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.Name))
                .ForMember(dest => dest.TickerSymbol, opt => opt.MapFrom(src => src.Company.TickerSymbol))
                .ForMember(dest => dest.SentimentLabel, opt => opt.MapFrom(src => src.Analysis != null ? src.Analysis.Sentiment.ToString() : string.Empty))
                .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => src.QuantMetrics != null ? src.QuantMetrics.EventType.ToString() : string.Empty))
                .ForMember(dest => dest.ExpectedDirection, opt => opt.MapFrom(src => src.QuantMetrics != null ? src.QuantMetrics.ExpectedDirection.ToString() : string.Empty))
                .ForMember(dest => dest.TimeHorizon, opt => opt.MapFrom(src => src.QuantMetrics != null ? src.QuantMetrics.TimeHorizon.ToString() : string.Empty));

            // Request -> NewsLog (Value Object oluşturma)
            CreateMap<CreateNewsLogRequest, NewsLog>()
                .ForMember(dest => dest.PublishedDate, opt => opt.MapFrom(src => src.PublishedDate.ToUniversalTime()))
                .AfterMap((src, dest) =>
                {
                    // String gelen değerleri enum'a çevir ve Value Object oluştur
                    if (!string.IsNullOrEmpty(src.SentimentLabel) || src.ConfidenceScore > 0)
                    {
                        var sentiment = ParseSentiment(src.SentimentLabel);
                        var analysis = AiAnalysisResult.Create(
                            src.IsTrendTriggered,
                            src.TrendSummary,
                            sentiment,
                            src.ConfidenceScore
                        );

                        var eventType = ParseEventType(src.EventType);
                        var direction = ParseDirection(src.ExpectedDirection);
                        var timeHorizon = ParseTimeHorizon(src.TimeHorizon);

                        var quantMetrics = QuantSignalMetrics.Create(
                            eventType,
                            src.ImpactStrength,
                            direction,
                            timeHorizon,
                            src.OverextendedRisk
                        );

                        dest.UpdateWithAiAnalysis(analysis, quantMetrics);
                    }
                });

            // PriceHistory Eşleştirmeleri
            CreateMap<PriceHistory, PriceHistoryResponse>();
            CreateMap<CreatePriceHistoryRequest, PriceHistory>()
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date.ToUniversalTime()));

            // EventTechnicalSnapshot Eşleştirmeleri
            CreateMap<EventTechnicalSnapshot, EventTechnicalSnapshotResponse>();
            CreateMap<CreateEventTechnicalSnapshotRequest, EventTechnicalSnapshot>();
        }

        // Yardımcı Parse Metodları
        private static SentimentType ParseSentiment(string value)
        {
            if (string.IsNullOrEmpty(value)) return SentimentType.Neutral;

            return value.ToLower() switch
            {
                "positive" => SentimentType.Positive,
                "negative" => SentimentType.Negative,
                _ => SentimentType.Neutral
            };
        }

        private static NewsEventType ParseEventType(string value)
        {
            if (string.IsNullOrEmpty(value)) return NewsEventType.Other;

            // Enum isimleriyle eşleştir
            if (Enum.TryParse<NewsEventType>(value, true, out var result))
                return result;

            // Kısmi eşleşmeler
            var lower = value.ToLower();
            if (lower.Contains("earning")) return NewsEventType.Earnings;
            if (lower.Contains("fda") || lower.Contains("approval")) return NewsEventType.FDAApproval;
            if (lower.Contains("upgrade")) return NewsEventType.Upgrade;
            if (lower.Contains("downgrade")) return NewsEventType.Downgrade;
            if (lower.Contains("hype") || lower.Contains("viral")) return NewsEventType.Hype;
            if (lower.Contains("partnership")) return NewsEventType.Partnership;
            if (lower.Contains("product") || lower.Contains("launch")) return NewsEventType.ProductLaunch;
            if (lower.Contains("regulatory")) return NewsEventType.Regulatory;
            if (lower.Contains("market")) return NewsEventType.MarketMove;

            return NewsEventType.Other;
        }

        private static DirectionType ParseDirection(string value)
        {
            if (string.IsNullOrEmpty(value)) return DirectionType.Uncertain;

            return value.ToLower() switch
            {
                "up" => DirectionType.Up,
                "down" => DirectionType.Down,
                _ => DirectionType.Uncertain
            };
        }

        private static TimeHorizonType ParseTimeHorizon(string value)
        {
            if (string.IsNullOrEmpty(value)) return TimeHorizonType.ShortTerm;

            return value.ToLower() switch
            {
                "intraday" => TimeHorizonType.Intraday,
                "shortterm" or "short_term" => TimeHorizonType.ShortTerm,
                "longterm" or "long_term" => TimeHorizonType.LongTerm,
                _ => TimeHorizonType.ShortTerm
            };
        }
    }
}