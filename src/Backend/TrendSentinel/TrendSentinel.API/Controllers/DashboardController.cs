using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TrendSentinel.Application.DTOs;
using TrendSentinel.Application.Interfaces;

namespace TrendSentinel.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly ISignalTrackService _signalTrackService;

        public DashboardController(ISignalTrackService signalTrackService)
        {
            _signalTrackService = signalTrackService;
        }


        /// Heatmap için optimize edilmiş aktif sinyal listesi
        /// GET: api/dashboard/heatmap
        [HttpGet("heatmap")]
        public async Task<IActionResult> GetHeatmapData()
        {
            var signals = await _signalTrackService.GetActiveSignalsForHeatmapAsync();
            return Ok(signals);
        }

        /// Dashboard özet istatistikleri + sinyal listesi
        /// GET: api/dashboard/summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var summary = await _signalTrackService.GetDashboardSummaryAsync();
            return Ok(summary);
        }
    }
}