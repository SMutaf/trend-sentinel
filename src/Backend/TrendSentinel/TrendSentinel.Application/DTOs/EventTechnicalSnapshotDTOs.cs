using System;

namespace TrendSentinel.Application.DTOs
{
    public class CreateEventTechnicalSnapshotRequest
    {
        public Guid NewsLogId { get; set; }

        public decimal RsiValue { get; set; }
        public string MacdState { get; set; } = string.Empty;
        public int TechScore { get; set; }
        public bool IsOverextended { get; set; }
        public double VolRatio { get; set; }
        public string VolTrend { get; set; }
        public int AboveAvgDaysLast5 { get; set; }
    }

    public class EventTechnicalSnapshotResponse
    {
        public Guid Id { get; set; }
        public Guid NewsLogId { get; set; }

        public decimal RsiValue { get; set; }
        public string MacdState { get; set; } = string.Empty;
        public int TechScore { get; set; }
        public bool IsOverextended { get; set; }
        public double VolRatio { get; set; }
        public string VolTrend { get; set; }
        public int AboveAvgDaysLast5 { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}