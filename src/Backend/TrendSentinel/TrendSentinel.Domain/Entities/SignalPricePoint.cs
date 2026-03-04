using System;
using TrendSentinel.Domain.Common;

namespace TrendSentinel.Domain.Entities
{
    /// Bir SignalTrack'in günlük fiyat kaydı.
    /// Backtest tekrarlanabilirliği için veritabanında saklanır.
    /// DayNumber: Sinyal girişine göre gün sayısı (0 = giriş günü)
    public class SignalPricePoint : BaseEntity
    {
        public Guid SignalTrackId { get; set; }
        public virtual SignalTrack SignalTrack { get; set; } = null!;

        // === ZAMAN BİLGİSİ ===
        public DateTime Date { get; set; }        // Gerçek tarih (2024-03-15)
        public int DayNumber { get; set; }        // Sinyale göre gün: 0 (giriş), 1, 2... 10

        // === OHLCV VERİSİ ===
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }        // Kapanış fiyatı (hesaplama için anahtar)
        public long Volume { get; set; }

        // === HESAPLANMIŞ ALANLAR (Worker tarafından doldurulur) ===
        public decimal CumulativeChangePercent { get; set; } // EntryPrice'a göre toplam değişim
    }
}