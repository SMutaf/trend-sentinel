using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrendSentinel.Application.DTOs;

namespace TrendSentinel.Application.Interfaces
{
    public interface ISignalPricePointService
    {
        Task<SignalPricePointResponse> AddPricePointAsync(CreateSignalPricePointRequest request);
        Task<List<SignalPricePointResponse>> GetPricePointsBySignalIdAsync(Guid signalId);
    }
}