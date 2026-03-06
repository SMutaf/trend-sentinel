using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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

            // sektör bilgisi
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

            // Trend tetiklendiyse Telegram alert'i hazırla ve gönder
            if (request.IsTrendTriggered)
            {
                //  Include ile tek sorguda tüm verileri çek
                var logWithIncludes = await _newsLogRepository.GetByIdWithIncludesAsync(
                    createdLog.Id,
                    n => n.Company,
                    n => n.TechnicalSnapshot,
                    n => n.PriceSnapshot
                );

                var alertMessage = FormatProfessionalAlert(logWithIncludes);

                // Fire-and-forget: Telegram servisi ana akışı bloklamasın
                _ = _telegramService.SendAlertAsync(alertMessage);
            }

            return _mapper.Map<NewsLogResponse>(createdLog);
        }

        public async Task<List<NewsLogResponse>> GetNewsLogsByCompanyIdAsync(Guid companyId)
        {
            var logs = await _newsLogRepository.GetAsync(n => n.CompanyId == companyId);
            return _mapper.Map<List<NewsLogResponse>>(logs);
        }

        //  Profesyonel alert formatlama metodu
        private string FormatProfessionalAlert(NewsLog log)
        {
            var publishedAt = log.PublishedDate.ToString("dd MMM yyyy HH:mm");
            var sentimentEmoji = log.SentimentLabel.ToLower() switch
            {
                "positive" => "🟢",
                "negative" => "🔴",
                "neutral" => "🟡",
                _ => "⚪"
            };

            var directionEmoji = log.ExpectedDirection?.ToLower() switch
            {
                "up" => "📈",
                "down" => "📉",
                _ => "➡️"
            };

            var message = $"*{sentimentEmoji} YAPAY ZEKA TREND ALARMI {directionEmoji}*\n\n";

            // Company 
            message += $"* Şirket:* {log.Company?.TickerSymbol ?? "N/A"} ({log.Company?.Name ?? "Bilinmiyor"})\n";
            message += $"* Başlık:* {log.Title}\n";
            message += $"* Yayın Tarihi:* `{publishedAt}`\n\n";

            // AI Analiz Bölümü
            message += $"* AI Temel Analiz*\n";
            message += $"_Trend:_ {log.TrendSummary}\n";
            message += $"*Sentiment:* {log.SentimentLabel} | *Güven:* %{log.ConfidenceScore}\n";
            message += $"*Etki:* {log.ImpactStrength}/5 | *Tip:* {log.EventType}\n";
            message += $"*Beklenen Yön:* {log.ExpectedDirection} | *Süre:* {log.TimeHorizon}\n\n";

            // teknik Analiz Bölümü (Varsa)
            if (log.TechnicalSnapshot != null)
            {
                var tech = log.TechnicalSnapshot;
                var rsiStatus = tech.RsiValue > 70 ? "*(Aşırı Alım)*" :
                                tech.RsiValue < 30 ? "*(Aşırı Satım)*" : "";

                message += $"* Teknik Göstergeler (Olay Anı)*\n";
                message += $"*RSI:* {tech.RsiValue:F1} {rsiStatus}\n";
                message += $"*MACD:* `{tech.MacdState}`\n";
                message += $"*Teknik Skor:* {tech.TechScore}/100\n";
                message += $"*Aşırı Uzama Riski:* {(tech.IsOverextended ? "EVET" : "Hayır")}\n";

                // Volume Analizi
                message += $"\n*🔊 Hacim Analizi*\n";
                message += $"*VolRatio:* {tech.VolRatio:F2}x | *Trend:* {tech.VolTrend}\n";
                message += $"*Ort. Üstü Gün (Son 5):* {tech.AboveAvgDaysLast5}/5\n";
            }

            // Fiyat Bilgisi (Varsa)
            if (log.PriceSnapshot != null)
            {
                var price = log.PriceSnapshot;
                message += $"\n* Fiyat Bilgisi (Olay Anı)*\n";
                message += $"*Kapanış:* ${price.Close:F2} | *Hacim:* {price.Volume:N0}\n";
            }

            // Link ve Footer
            message += $"\n [Haberi İncele]({log.Url})\n";

            return message;
        }
    }
}