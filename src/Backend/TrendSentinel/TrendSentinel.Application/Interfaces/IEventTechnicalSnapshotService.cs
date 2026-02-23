using System;
using System.Threading.Tasks;
using TrendSentinel.Application.DTOs;

namespace TrendSentinel.Application.Interfaces
{
    public interface IEventTechnicalSnapshotService
    {
        Task<EventTechnicalSnapshotResponse> AddSnapshotAsync(CreateEventTechnicalSnapshotRequest request);

        Task<EventTechnicalSnapshotResponse?> GetSnapshotByNewsLogIdAsync(Guid newsLogId);
    }
}