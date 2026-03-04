using AutoMapper;
using TrendSentinel.Domain.Entities;
using TrendSentinel.Application.DTOs;

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

            // NewsLog Eşleştirmeleri
            CreateMap<NewsLog, NewsLogResponse>();
            CreateMap<CreateNewsLogRequest, NewsLog>()
                .ForMember(dest => dest.PublishedDate, opt => opt.MapFrom(src => src.PublishedDate.ToUniversalTime()));

            // PriceHistory Eşleştirmesi
            CreateMap<PriceHistory, PriceHistoryResponse>();
            CreateMap<CreatePriceHistoryRequest, PriceHistory>()
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date.ToUniversalTime()));

            CreateMap<CreateEventTechnicalSnapshotRequest, EventTechnicalSnapshot>();
            CreateMap<EventTechnicalSnapshot, EventTechnicalSnapshotResponse>();

            // === SignalTrack Mapping'leri ===
            CreateMap<SignalTrack, SignalTrackResponse>()
                .ForMember(dest => dest.TickerSymbol, opt => opt.MapFrom(src => src.Company.TickerSymbol))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.Name))
                .ForMember(dest => dest.TrendSummary, opt => opt.MapFrom(src => src.NewsLog.TrendSummary))
                .ForMember(dest => dest.ConfidenceScore, opt => opt.MapFrom(src => src.NewsLog.ConfidenceScore));

            CreateMap<CreateSignalTrackRequest, SignalTrack>();

            // Heatmap için optimize mapping
            CreateMap<SignalTrack, SignalHeatmapItem>()
                .ForMember(dest => dest.TickerSymbol, opt => opt.MapFrom(src => src.Company.TickerSymbol))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.Name))
                .ForMember(dest => dest.PerformancePercent, opt => opt.MapFrom(src => src.PerformancePercent ?? 0));

            // === SignalPricePoint Mapping'leri ===
            CreateMap<SignalPricePoint, SignalPricePointResponse>();
            CreateMap<CreateSignalPricePointRequest, SignalPricePoint>();

            // Grafik için mapping
            CreateMap<SignalPricePoint, SignalChartPoint>()
                .ForMember(dest => dest.Day, opt => opt.MapFrom(src => src.DayNumber))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Close))
                .ForMember(dest => dest.Change, opt => opt.MapFrom(src => src.CumulativeChangePercent));
        }
    }
    
}