using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrendSentinel.Application.DTOs;
using TrendSentinel.Application.Interfaces;
using TrendSentinel.Domain.Entities;
using TrendSentinel.Domain.Enums;
using TrendSentinel.Domain.Interfaces;

namespace TrendSentinel.Application.Services
{
    public class SignalTrackService : ISignalTrackService
    {
        private readonly IAsyncRepository<SignalTrack> _signalRepository;
        private readonly IAsyncRepository<NewsLog> _newsLogRepository;
        private readonly IAsyncRepository<SignalPricePoint> _pricePointRepository;
        private readonly IMapper _mapper;

        public SignalTrackService(
            IAsyncRepository<SignalTrack> signalRepository,
            IAsyncRepository<NewsLog> newsLogRepository,
            IAsyncRepository<SignalPricePoint> pricePointRepository,
            IMapper mapper)
        {
            _signalRepository = signalRepository;
            _newsLogRepository = newsLogRepository;
            _pricePointRepository = pricePointRepository;
            _mapper = mapper;
        }

        public async Task<SignalTrackResponse> CreateSignalTrackAsync(CreateSignalTrackRequest request)
        {
            var newsLogs = await _newsLogRepository.GetAsync(n => n.Id == request.NewsLogId);
            var newsLog = newsLogs.FirstOrDefault();

            if (newsLog == null)
                throw new ArgumentException($"NewsLog bulunamadı: {request.NewsLogId}");

            var signalTrack = _mapper.Map<SignalTrack>(request);
            signalTrack.CompanyId = newsLog.CompanyId;
            signalTrack.EntryDate = DateTime.UtcNow;
            signalTrack.Status = SignalStatus.Active;
            signalTrack.CurrentPrice = request.EntryPrice; // Entry'i current olarak başlat

            var created = await _signalRepository.AddAsync(signalTrack);
            return await GetSignalTrackResponseAsync(created);
        }

        public async Task<SignalTrackResponse?> GetSignalTrackByIdAsync(Guid id)
        {
            var signals = await _signalRepository.GetAsync(s => s.Id == id);
            var signal = signals.FirstOrDefault();

            if (signal == null) return null;

            return await GetSignalTrackResponseAsync(signal);
        }

        public async Task<List<SignalTrackResponse>> GetSignalTracksByCompanyIdAsync(Guid companyId)
        {
            var signals = await _signalRepository.GetAsync(s => s.CompanyId == companyId);

            var responses = new List<SignalTrackResponse>();
            foreach (var signal in signals)
            {
                responses.Add(await GetSignalTrackResponseAsync(signal));
            }

            return responses;
        }

        public async Task<List<SignalHeatmapItem>> GetActiveSignalsForHeatmapAsync()
        {
            // ✅ ÖNEMLİ: Company navigation property'yi yükle
            var signals = await _signalRepository.GetAsync(s => s.Status == SignalStatus.Active);

            var heatmapItems = new List<SignalHeatmapItem>();

            foreach (var signal in signals)
            {
                heatmapItems.Add(new SignalHeatmapItem
                {
                    Id = signal.Id,
                    TickerSymbol = signal.Company?.TickerSymbol ?? "UNKNOWN",
                    CompanyName = signal.Company?.Name ?? "UNKNOWN",
                    PerformancePercent = signal.PerformancePercent ?? 0,
                    DaysElapsed = signal.DaysElapsed,
                    Status = signal.Status,
                    EntryPrice = signal.EntryPrice,
                    CurrentPrice = signal.CurrentPrice ?? signal.EntryPrice // Null ise EntryPrice kullan
                });
            }

            return heatmapItems;
        }

        public async Task<DashboardHeatmapResponse> GetDashboardSummaryAsync()
        {
            var activeSignals = await GetActiveSignalsForHeatmapAsync();

            var stats = new DashboardStats
            {
                TotalActiveSignals = activeSignals.Count,
                AvgPerformance = activeSignals.Count > 0
                    ? activeSignals.Average(s => s.PerformancePercent)
                    : 0,
                SignalsExpired = 0
            };

            return new DashboardHeatmapResponse
            {
                ActiveSignals = activeSignals,
                Stats = stats
            };
        }

        public async Task<List<SignalChartPoint>> GetChartPointsForSignalAsync(Guid signalId)
        {
            var points = await _pricePointRepository.GetAsync(p => p.SignalTrackId == signalId);
            return _mapper.Map<List<SignalChartPoint>>(points);
        }

        public async Task<bool> CloseSignalManuallyAsync(Guid signalId)
        {
            var signals = await _signalRepository.GetAsync(s => s.Id == signalId);
            var signal = signals.FirstOrDefault();

            if (signal == null) return false;

            signal.Status = SignalStatus.Cancelled;
            signal.ClosedDate = DateTime.UtcNow;

            await _signalRepository.UpdateAsync(signal);
            return true;
        }

        // === PRIVATE HELPER ===
        private async Task<SignalTrackResponse> GetSignalTrackResponseAsync(SignalTrack signal)
        {
            var response = _mapper.Map<SignalTrackResponse>(signal);

            // Company bilgilerini doldur
            if (signal.Company != null)
            {
                response.TickerSymbol = signal.Company.TickerSymbol;
                response.CompanyName = signal.Company.Name;
            }
            else
            {
                response.TickerSymbol = "UNKNOWN";
                response.CompanyName = "UNKNOWN";
            }

            // NewsLog bilgilerini doldur
            if (signal.NewsLog != null)
            {
                response.TrendSummary = signal.NewsLog.TrendSummary;
                response.ConfidenceScore = signal.NewsLog.ConfidenceScore;
            }

            // CurrentPrice null ise EntryPrice kullan
            if (response.CurrentPrice == null)
            {
                response.CurrentPrice = response.EntryPrice;
            }

            return response;
        }
    }
}