using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TrendSentinel.Application.DTOs;
using TrendSentinel.Application.Interfaces;

namespace TrendSentinel.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompaniesController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _companyService.GetAllCompaniesAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCompanyRequest request)
        {
            var result = await _companyService.CreateCompanyAsync(request);
            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
        }
    }
}