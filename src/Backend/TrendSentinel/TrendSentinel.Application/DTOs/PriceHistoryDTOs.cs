using System;

namespace TrendSentinel.Application.DTOs
{
    public class CreatePriceHistoryRequest
    {
        public Guid CompanyId { get; set; }
        public DateTime Date { get; set; }

        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
    }

    public class PriceHistoryResponse
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public DateTime Date { get; set; }

        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
    }
}