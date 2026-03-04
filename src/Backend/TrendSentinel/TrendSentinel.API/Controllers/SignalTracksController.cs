using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TrendSentinel.Application.DTOs;
using TrendSentinel.Application.Interfaces;

namespace TrendSentinel.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignalTracksController : ControllerBase
    {
        private readonly ISignalTrackService _signalTrackService;
        private readonly ISignalPricePointService _pricePointService;

        public SignalTracksController(
            ISignalTrackService signalTrackService,
            ISignalPricePointService pricePointService)
        {
            _signalTrackService = signalTrackService;
            _pricePointService = pricePointService;
        }


        /// Yeni bir trend sinyali için takip kaydı oluştur
        /// POST: api/signaltracks
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSignalTrackRequest request)
        {
            var result = await _signalTrackService.CreateSignalTrackAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// Sinyal detaylarını getir
        /// GET: api/signaltracks/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var signal = await _signalTrackService.GetSignalTrackByIdAsync(id);
            if (signal == null) return NotFound();
            return Ok(signal);
        }

        /// Sinyale ait fiyat geçmişini getir (grafik için)
        /// GET: api/signaltracks/{id}/prices
        [HttpGet("{id}/prices")]
        public async Task<IActionResult> GetPriceHistory(Guid id)
        {
            var chartPoints = await _signalTrackService.GetChartPointsForSignalAsync(id);
            return Ok(chartPoints);
        }

        /// Şirkete ait tüm sinyalleri listele
        /// GET: api/signaltracks/company/{companyId}
        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetByCompany(Guid companyId)
        {
            var signals = await _signalTrackService.GetSignalTracksByCompanyIdAsync(companyId);
            return Ok(signals);
        }

        /// Sinyali manuel olarak kapat (admin/debug)
        /// PATCH: api/signaltracks/{id}/close
        [HttpPatch("{id}/close")]
        public async Task<IActionResult> CloseSignal(Guid id)
        {
            var result = await _signalTrackService.CloseSignalManuallyAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}