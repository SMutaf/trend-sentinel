using System;
using System.Collections.Generic;
using TrendSentinel.Domain.Common;
using TrendSentinel.Domain.Enums;

namespace TrendSentinel.Domain.Entities
{
    /// Bir trend sinyalinin takip kaydı.
    /// NewsLog tetiklendiğinde (IsTrendTriggered=true) oluşturulur,
    /// TargetDurationDays boyunca izlenir ve performansı hesaplanır.
    public class SignalTrack : BaseEntity
    {
        // === İLİŞKİLER ===
        public Guid NewsLogId { get; set; }
        public virtual NewsLog NewsLog { get; set; } = null!;

        public Guid CompanyId { get; set; }
        public virtual Company Company { get; set; } = null!;

        // === GİRİŞ NOKTASI (Sinyal anı - sabit) ===
        public decimal EntryPrice { get; set; }           // Sinyal tetiklendiğindeki fiyat
        public DateTime EntryDate { get; set; }           // Takip başlangıç tarihi (UTC)

        // === ANLIK DURUM (Günlük güncellenir) ===
        public decimal? CurrentPrice { get; set; }        // En son çekilen fiyat
        public decimal? PerformancePercent { get; set; }  // ((Current - Entry) / Entry) * 100
        public int DaysElapsed { get; set; }              // Sinyalden beri geçen gün sayısı

        // === KONFİGÜRASYON ===
        public int TargetDurationDays { get; set; } = 10; // Kaç gün takip edilsin?

        // === STATÜ YÖNETİMİ ===
        public SignalStatus Status { get; set; } = SignalStatus.Active;
        public DateTime? ClosedDate { get; set; }         // Sinyal kapanış tarihi

        // === PERFORMANS METRİKLERİ (Backtest analizi) ===
        public decimal? MaxPerformancePercent { get; set; } // Ulaşılan en yüksek kar %
        public int PeakDay { get; set; }                    // En yüksek kar hangi gün? (0-based)

        // === GÜNLÜK FİYAT NOKTALARI ===
        public virtual ICollection<SignalPricePoint> PricePoints { get; set; }
            = new List<SignalPricePoint>();
    }
}