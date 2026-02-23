using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrendSentinel.Application.DTOs;
using TrendSentinel.Application.Interfaces;

namespace TrendSentinel.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventTechnicalSnapshotsController : ControllerBase
    {
        private readonly IEventTechnicalSnapshotService _snapshotService;

        public EventTechnicalSnapshotsController(IEventTechnicalSnapshotService snapshotService)
        {
            _snapshotService = snapshotService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSnapshot([FromBody] CreateEventTechnicalSnapshotRequest request)
        {
            var response = await _snapshotService.AddSnapshotAsync(request);
            return Ok(response);
        }

        [HttpGet("newslog/{newsLogId}")]
        public async Task<IActionResult> GetByNewsLogId(Guid newsLogId)
        {
            var response = await _snapshotService.GetSnapshotByNewsLogIdAsync(newsLogId);

            if (response == null)
                return NotFound();

            return Ok(response);
        }
    }
}