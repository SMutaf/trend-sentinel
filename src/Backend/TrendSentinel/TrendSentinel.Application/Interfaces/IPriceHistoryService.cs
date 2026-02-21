using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrendSentinel.Application.DTOs;

namespace TrendSentinel.Application.Interfaces
{
    public interface IPriceHistoryService
    {
        Task<PriceHistoryResponse> AddSnapshotAsync(CreatePriceHistoryRequest request);
        Task<IReadOnlyList<PriceHistoryResponse>> GetHistoryByCompanyIdAsync(Guid companyId);
    }
}