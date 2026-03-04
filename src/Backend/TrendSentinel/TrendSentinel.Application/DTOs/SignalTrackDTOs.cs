using System;
using TrendSentinel.Domain.Enums;

namespace TrendSentinel.Application.DTOs
{
    // === CREATE REQUEST ===
    public class CreateSignalTrackRequest
    {
        public Guid NewsLogId { get; set; }
        public decimal EntryPrice { get; set; }
        public int TargetDurationDays { get; set; } = 10;
    }

    // === RESPONSE (Detay) ===
    public class SignalTrackResponse
    {
        public Guid Id { get; set; }
        public Guid NewsLogId { get; set; }
        public Guid CompanyId { get; set; }
        public string TickerSymbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;

        public decimal EntryPrice { get; set; }
        public DateTime EntryDate { get; set; }
        public decimal? CurrentPrice { get; set; }
        public decimal? PerformancePercent { get; set; }
        public int DaysElapsed { get; set; }

        public int TargetDurationDays { get; set; }
        public SignalStatus Status { get; set; }
        public DateTime? ClosedDate { get; set; }

        public decimal? MaxPerformancePercent { get; set; }
        public int PeakDay { get; set; }

        // AI'dan gelen bağlam (Heatmap tooltip için)
        public string? TrendSummary { get; set; }
        public int ConfidenceScore { get; set; }
    }

    // === HEATMAP ÖZEL (Frontend için optimize) ===
    public class SignalHeatmapItem
    {
        public Guid Id { get; set; }
        public string TickerSymbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;

        // Heatmap rengi için KRİTİK alanlar
        public decimal PerformancePercent { get; set; }
        public int DaysElapsed { get; set; }
        public SignalStatus Status { get; set; }

        // Tooltip için minimal veri
        public decimal EntryPrice { get; set; }
        public decimal? CurrentPrice { get; set; }
    }

    // === DASHBOARD SUMMARY ===
    public class DashboardHeatmapResponse
    {
        public List<SignalHeatmapItem> ActiveSignals { get; set; } = new();
        public DashboardStats Stats { get; set; } = new();
    }

    public class DashboardStats
    {
        public int TotalActiveSignals { get; set; }
        public decimal AvgPerformance { get; set; }
        public int SignalsExpired { get; set; }
    }
}