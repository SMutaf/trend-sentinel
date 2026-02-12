using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TrendSentinel.Application.DTOs;
using TrendSentinel.Application.Interfaces;

namespace TrendSentinel.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsLogsController : ControllerBase
    {
        private readonly INewsLogService _newsLogService;

        public NewsLogsController(INewsLogService newsLogService)
        {
            _newsLogService = newsLogService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateNewsLogRequest request)
        {
            var result = await _newsLogService.CreateNewsLogAsync(request);
            return Ok(result);
        }

        [HttpGet("{companyId}")]
        public async Task<IActionResult> GetByCompany(Guid companyId)
        {
            var result = await _newsLogService.GetNewsLogsByCompanyIdAsync(companyId);
            return Ok(result);
        }
    }
}