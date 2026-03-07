using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrendSentinel.Application.DTOs;
using TrendSentinel.Application.Interfaces;
using TrendSentinel.Domain.Entities;
using TrendSentinel.Domain.Interfaces;
using TrendSentinel.Domain.Enums;

namespace TrendSentinel.Application.Services
{
    public class NewsLogService : INewsLogService
    {
        private readonly IAsyncRepository<NewsLog> _newsLogRepository;
        private readonly ITelegramService _telegramService;
        private readonly IAsyncRepository<Company> _companyRepository;
        private readonly IMapper _mapper;

        public NewsLogService(
            IAsyncRepository<NewsLog> newsLogRepository,
            IAsyncRepository<Company> companyRepository,
            ITelegramService telegramService,
            IMapper mapper)
        {
            _newsLogRepository = newsLogRepository;
            _telegramService = telegramService;
            _companyRepository = companyRepository;
            _mapper = mapper;
        }

        public async Task<NewsLogResponse> CreateNewsLogAsync(CreateNewsLogRequest request)
        {
            var newLog = _mapper.Map<NewsLog>(request);
            var createdLog = await _newsLogRepository.AddAsync(newLog);

            // Sektör bilgisi güncelleme
            if (request.SectorId > 0)
            {
                var companies = await _companyRepository.GetAsync(c => c.Id == request.CompanyId);
                var company = companies.FirstOrDefault();
                if (company != null && company.Sector == SectorType.Other)
                {
                    company.Sector = (SectorType)request.SectorId;
                    await _companyRepository.UpdateAsync(company);
                }
            }

            // Trend tetiklendiyse Telegram alert'i gönder
            if (createdLog.ShouldTriggerAlert())
            {
                var logWithIncludes = await _newsLogRepository.GetByIdWithIncludesAsync(
                    createdLog.Id,
                    n => n.Company,
                    n => n.TechnicalSnapshot,
                    n => n.PriceSnapshot
                );

                if (logWithIncludes != null)
                {
                    var alertMessage = FormatProfessionalAlert(logWithIncludes);
                    _ = _telegramService.SendAlertAsync(alertMessage);
                }
            }

            return _mapper.Map<NewsLogResponse>(createdLog);
        }

        public async Task<List<NewsLogResponse>> GetNewsLogsByCompanyIdAsync(Guid companyId, int limit = 5)
        {
            var logs = await _newsLogRepository.GetWithIncludesAsync(
                n => n.CompanyId == companyId,
                n => n.Company,
                n => n.TechnicalSnapshot,
                n => n.PriceSnapshot
            );

            var recentLogs = logs.OrderByDescending(n => n.CreatedDate).Take(limit).ToList();
            return _mapper.Map<List<NewsLogResponse>>(recentLogs);
        }

        // Profesyonel alert formatlama metodu
        private string FormatProfessionalAlert(NewsLog log)
        {
            var publishedAt = log.PublishedDate.ToString("dd MMM yyyy HH:mm");

            // Value Object'ten sentiment al
            var sentiment = log.Analysis?.Sentiment ?? SentimentType.Neutral;
            var sentimentEmoji = sentiment switch
            {
                SentimentType.Positive => "🟢",
                SentimentType.Negative => "🔴",
                _ => "🟡"
            };

            // Value Object'ten direction al
            var direction = log.QuantMetrics?.ExpectedDirection ?? DirectionType.Uncertain;
            var directionEmoji = direction switch
            {
                DirectionType.Up => "📈",
                DirectionType.Down => "📉",
                _ => "➡️"
            };

            var message = $"*{sentimentEmoji} YAPAY ZEKA TREND ALARMI {directionEmoji}*\n";

            // Company
            message += $"* Şirket:* {log.Company?.TickerSymbol ?? "N/A"} ({log.Company?.Name ?? "Bilinmiyor"})\n";
            message += $"* Başlık:* {log.Title}\n";
            message += $"* Yayın Tarihi:* `{publishedAt}`\n\n";

            // AI Analiz Bölümü
            if (log.Analysis != null)
            {
                message += $"*🤖 AI Temel Analiz*\n";
                message += $"_Trend:_ {log.Analysis.TrendSummary}\n";
                message += $"*Sentiment:* {log.Analysis.Sentiment} | *Güven:* %{log.Analysis.ConfidenceScore}\n";
            }

            // Quant Bölümü
            if (log.QuantMetrics != null)
            {
                var metrics = log.QuantMetrics;
                message += $"*📊 Algoritmik Sinyal*\n";
                message += $"*Etki:* {metrics.ImpactStrength}/5 | *Tip:* {metrics.EventType}\n";
                message += $"*Beklenen Yön:* {metrics.ExpectedDirection} | *Süre:* {metrics.TimeHorizon}\n";
                message += $"*Şişme Riski:* {(metrics.OverextendedRisk ? "⚠️ EVET" : "✅ Hayır")}\n";
            }

            // Teknik Analiz Bölümü (Varsa)
            if (log.TechnicalSnapshot != null)
            {
                var tech = log.TechnicalSnapshot;
                var rsiStatus = tech.RsiValue > 70 ? "*(Aşırı Alım)*" :
                                tech.RsiValue < 30 ? "*(Aşırı Satım)*" : "";

                message += $"\n*📈 Teknik Göstergeler (Olay Anı)*\n";
                message += $"*RSI:* {tech.RsiValue:F1} {rsiStatus}\n";
                message += $"*MACD:* `{tech.MacdState}`\n";
                message += $"*Teknik Skor:* {tech.TechScore}/100\n";
                message += $"*Aşırı Uzama:* {(tech.IsOverextended ? "⚠️ EVET" : "✅ Hayır")}\n";

                // Volume Analizi
                if (tech.VolRatio > 0)
                {
                    message += $"\n*🔊 Hacim Analizi*\n";
                    message += $"*VolRatio:* {tech.VolRatio:F2}x | *Trend:* {tech.VolTrend}\n";
                    message += $"*Ort. Üstü Gün (Son 5):* {tech.AboveAvgDaysLast5}/5\n";
                }
            }

            // Fiyat Bilgisi (Varsa)
            if (log.PriceSnapshot != null)
            {
                var price = log.PriceSnapshot;
                message += $"\n*💰 Fiyat Bilgisi (Olay Anı)*\n";
                message += $"*Kapanış:* ${price.Close:F2} | *Hacim:* {price.Volume:N0}\n";
            }

            // Link ve Footer
            message += $"\n[Haberi İncele]({log.Url})\n";

            return message;
        }
    }
}