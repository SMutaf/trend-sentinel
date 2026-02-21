using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using TrendSentinel.Application.DTOs;
using TrendSentinel.Application.Interfaces;
using TrendSentinel.Domain.Entities;
using TrendSentinel.Domain.Interfaces;

namespace TrendSentinel.Application.Services
{
    public class PriceHistoryService : IPriceHistoryService
    {
        private readonly IAsyncRepository<PriceHistory> _repository;
        private readonly IMapper _mapper;

        public PriceHistoryService(IAsyncRepository<PriceHistory> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PriceHistoryResponse> AddSnapshotAsync(CreatePriceHistoryRequest request)
        {
            var priceHistory = _mapper.Map<PriceHistory>(request);

            var addedEntity = await _repository.AddAsync(priceHistory);

            return _mapper.Map<PriceHistoryResponse>(addedEntity);
        }

        public async Task<IReadOnlyList<PriceHistoryResponse>> GetHistoryByCompanyIdAsync(Guid companyId)
        {
            var histories = await _repository.GetAsync(p => p.CompanyId == companyId);

            return _mapper.Map<IReadOnlyList<PriceHistoryResponse>>(histories);
        }
    }
}