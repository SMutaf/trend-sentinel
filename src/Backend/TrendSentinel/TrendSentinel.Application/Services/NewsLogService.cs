using AutoMapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrendSentinel.Application.DTOs;
using TrendSentinel.Application.Interfaces;
using TrendSentinel.Domain.Entities;
using TrendSentinel.Domain.Interfaces;

namespace TrendSentinel.Application.Services
{
    public class NewsLogService : INewsLogService
    {
        private readonly IAsyncRepository<NewsLog> _newsLogRepository;
        private readonly ITelegramService _telegramService;
        private readonly IMapper _mapper;

        public NewsLogService(IAsyncRepository<NewsLog> newsLogRepository, ITelegramService telegramService, IMapper mapper)
        {
            _newsLogRepository = newsLogRepository;
            _telegramService = telegramService;
            _mapper = mapper;
        }

        public async Task<NewsLogResponse> CreateNewsLogAsync(CreateNewsLogRequest request)
        {
            var newLog = _mapper.Map<NewsLog>(request);
            var createdLog = await _newsLogRepository.AddAsync(newLog);

            // Trend KOntrolü eğer trend trigger olduysa bota mesaj at
            if (request.IsTrendTriggered)
            {
                var alertMessage = $"*YAPAY ZEKA TREND ALARMI!*\n\n" +
                                   $"*Başlık:* {request.Title}\n" +
                                   $"*Analiz:* _{request.TrendSummary}_\n" +
                                   $"Sentiment: {request.SentimentLabel}\n" +
                                   $"[Habere Git]({request.Url})";

                _ = _telegramService.SendAlertAsync(alertMessage);
            }

            return _mapper.Map<NewsLogResponse>(createdLog);
        }

        public async Task<List<NewsLogResponse>> GetNewsLogsByCompanyIdAsync(Guid companyId)
        {
            var logs = await _newsLogRepository.GetAsync(n => n.CompanyId == companyId);
            return _mapper.Map<List<NewsLogResponse>>(logs);
        }
    }
}