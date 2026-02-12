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
        }
    }
}