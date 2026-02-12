using System.Collections.Generic;
using System.Threading.Tasks;
using TrendSentinel.Application.DTOs;

namespace TrendSentinel.Application.Interfaces
{
    public interface ICompanyService
    {
        Task<List<CompanyResponse>> GetAllCompaniesAsync();
        Task<CompanyResponse> CreateCompanyAsync(CreateCompanyRequest request);
    }
}