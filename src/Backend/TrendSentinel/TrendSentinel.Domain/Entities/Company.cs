using System.Collections.Generic;
using TrendSentinel.Domain.Common;
using TrendSentinel.Domain.Enums;

namespace TrendSentinel.Domain.Entities
{
    public class Company : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string TickerSymbol { get; set; } = string.Empty; 
        public SectorType Sector { get; set; }

        // Alert gönderildi mi? (Sürekli spam yapmamak için)
        public bool IsAlertSent { get; set; }


        public ICollection<NewsLog> NewsLogs { get; set; }

        public Company()
        {
            NewsLogs = new List<NewsLog>();
        }
    }
}