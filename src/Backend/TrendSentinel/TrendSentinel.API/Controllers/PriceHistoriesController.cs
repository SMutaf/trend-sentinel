using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TrendSentinel.Application.DTOs;
using TrendSentinel.Application.Interfaces;

namespace TrendSentinel.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriceHistoriesController : ControllerBase
    {
        private readonly IPriceHistoryService _priceHistoryService;

        public PriceHistoriesController(IPriceHistoryService priceHistoryService)
        {
            _priceHistoryService = priceHistoryService;
        }

        [HttpPost]
        public async Task<IActionResult> AddSnapshot([FromBody] CreatePriceHistoryRequest request)
        {
            if (request == null)
                return BadRequest("Geçersiz fiyat verisi.");

            var response = await _priceHistoryService.AddSnapshotAsync(request);
            return Ok(response);
        }

        [HttpGet("newslog/{newsLogId}")]
        public async Task<IActionResult> GetByNewsLogId(Guid newsLogId)
        {
            var histories = await _priceHistoryService.GetSnapshotByNewsLogIdAsync(newsLogId);
            return Ok(histories);
        }
    }
}