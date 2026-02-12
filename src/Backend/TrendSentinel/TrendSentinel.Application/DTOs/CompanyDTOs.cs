using System;
using TrendSentinel.Domain.Enums;

namespace TrendSentinel.Application.DTOs
{
    public class CreateCompanyRequest
    {
        public string Name { get; set; } = string.Empty;
        public string TickerSymbol { get; set; } = string.Empty;
        public SectorType Sector { get; set; }
    }

    public class CompanyResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TickerSymbol { get; set; } = string.Empty;
        public SectorType Sector { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}