using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using TrendSentinel.Application.DTOs;
using TrendSentinel.Application.Interfaces;
using TrendSentinel.Domain.Entities;
using TrendSentinel.Domain.Interfaces;

namespace TrendSentinel.Application.Services
{
    public class EventTechnicalSnapshotService : IEventTechnicalSnapshotService
    {
        private readonly IAsyncRepository<EventTechnicalSnapshot> _repository;
        private readonly IMapper _mapper;

        public EventTechnicalSnapshotService(IAsyncRepository<EventTechnicalSnapshot> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<EventTechnicalSnapshotResponse> AddSnapshotAsync(CreateEventTechnicalSnapshotRequest request)
        {
            var snapshot = _mapper.Map<EventTechnicalSnapshot>(request);
            var addedEntity = await _repository.AddAsync(snapshot);

            return _mapper.Map<EventTechnicalSnapshotResponse>(addedEntity);
        }

        public async Task<EventTechnicalSnapshotResponse?> GetSnapshotByNewsLogIdAsync(Guid newsLogId)
        {
            var snapshots = await _repository.GetAsync(s => s.NewsLogId == newsLogId);
            var snapshot = snapshots.FirstOrDefault();

            if (snapshot == null)
                return null;

            return _mapper.Map<EventTechnicalSnapshotResponse>(snapshot);
        }
    }
}