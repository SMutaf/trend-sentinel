using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrendSentinel.Application.DTOs;

namespace TrendSentinel.Application.Interfaces
{
    public interface INewsLogService
    {
        Task<NewsLogResponse> CreateNewsLogAsync(CreateNewsLogRequest request);
        Task<List<NewsLogResponse>> GetNewsLogsByCompanyIdAsync(Guid companyId);
    }
}