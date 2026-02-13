using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TrendSentinel.Application.Interfaces;

namespace TrendSentinel.Application.Services
{
    public class TelegramService : ITelegramService
    {
        private readonly TelegramBotClient _botClient;
        private readonly string _chatId;

        public TelegramService(IConfiguration configuration)
        {
            var token = configuration["TelegramSettings:BotToken"];
            _chatId = configuration["TelegramSettings:ChatId"];

            if (!string.IsNullOrEmpty(token))
            {
                _botClient = new TelegramBotClient(token);
            }
        }

        public async Task SendAlertAsync(string message)
        {
            if (_botClient == null || string.IsNullOrEmpty(_chatId)) return;

            try
            {
                await _botClient.SendMessage(
                    chatId: _chatId,
                    text: message,
                    parseMode: ParseMode.Markdown
                );
            }
            catch (Exception ex)
            {
                // Telegram patlarsa ana program durmasın, sadece loglasın
                Console.WriteLine($"Telegram Hatası: {ex.Message}");
            }
        }
    }
}