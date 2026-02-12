using System.Threading.Tasks;

namespace TrendSentinel.Application.Interfaces
{
    public interface ITelegramService
    {
        Task SendAlertAsync(string message);
    }
}