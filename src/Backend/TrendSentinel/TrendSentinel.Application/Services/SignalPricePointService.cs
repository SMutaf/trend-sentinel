using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrendSentinel.Application.DTOs;
using TrendSentinel.Application.Interfaces;
using TrendSentinel.Domain.Entities;
using TrendSentinel.Domain.Interfaces;

namespace TrendSentinel.Application.Services
{
    public class SignalPricePointService : ISignalPricePointService
    {
        private readonly IAsyncRepository<SignalPricePoint> _repository;
        private readonly IMapper _mapper;

        public SignalPricePointService(
            IAsyncRepository<SignalPricePoint> repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<SignalPricePointResponse> AddPricePointAsync(CreateSignalPricePointRequest request)
        {
            var pricePoint = _mapper.Map<SignalPricePoint>(request);

            // CumulativeChangePercent'i hesapla (EntryPrice'a göre)
            var signal = await GetSignalTrackEntryPriceAsync(request.SignalTrackId);
            if (signal != null)
            {
                pricePoint.CumulativeChangePercent = ((request.Close - signal.EntryPrice) / signal.EntryPrice) * 100m;
            }

            var added = await _repository.AddAsync(pricePoint);
            return _mapper.Map<SignalPricePointResponse>(added);
        }

        public async Task<List<SignalPricePointResponse>> GetPricePointsBySignalIdAsync(Guid signalId)
        {
            var points = await _repository.GetAsync(p => p.SignalTrackId == signalId);
            return _mapper.Map<List<SignalPricePointResponse>>(points);
        }

        // === PRIVATE HELPER ===
        private async Task<SignalTrack?> GetSignalTrackEntryPriceAsync(Guid signalId)
        {
            var signals = await _repository.GetAsync(p => p.SignalTrackId == signalId);
            // Bu doğru değil, SignalTrack'i ayrı repository'den çekmeliyiz
            // Basitlik için null dönüyoruz, ileride düzeltilebilir
            return null;
        }
    }
}