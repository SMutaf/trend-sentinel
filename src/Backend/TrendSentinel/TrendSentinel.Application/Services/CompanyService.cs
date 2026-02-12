using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrendSentinel.Application.DTOs;
using TrendSentinel.Application.Interfaces;
using TrendSentinel.Domain.Entities;
using TrendSentinel.Domain.Interfaces;

namespace TrendSentinel.Application.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IAsyncRepository<Company> _companyRepository;
        private readonly IMapper _mapper;

        public CompanyService(IAsyncRepository<Company> companyRepository, IMapper mapper)
        {
            _companyRepository = companyRepository;
            _mapper = mapper;
        }

        public async Task<CompanyResponse> CreateCompanyAsync(CreateCompanyRequest request)
        {
            var newCompany = _mapper.Map<Company>(request);
            var createdCompany = await _companyRepository.AddAsync(newCompany);
            return _mapper.Map<CompanyResponse>(createdCompany);
        }

        public async Task<List<CompanyResponse>> GetAllCompaniesAsync()
        {
            var companies = await _companyRepository.GetAllAsync();
            return _mapper.Map<List<CompanyResponse>>(companies);
        }
    }
}