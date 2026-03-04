using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrendSentinel.Application.DTOs;

namespace TrendSentinel.Application.Interfaces
{
    public interface ISignalTrackService
    {
        // === CRUD ===
        Task<SignalTrackResponse> CreateSignalTrackAsync(CreateSignalTrackRequest request);
        Task<SignalTrackResponse?> GetSignalTrackByIdAsync(Guid id);
        Task<List<SignalTrackResponse>> GetSignalTracksByCompanyIdAsync(Guid companyId);

        // === HEATMAP / DASHBOARD ===
        Task<List<SignalHeatmapItem>> GetActiveSignalsForHeatmapAsync();
        Task<DashboardHeatmapResponse> GetDashboardSummaryAsync();

        // === DETAY ===
        Task<List<SignalChartPoint>> GetChartPointsForSignalAsync(Guid signalId);

        // === ADMIN ===
        Task<bool> CloseSignalManuallyAsync(Guid signalId);
    }
}