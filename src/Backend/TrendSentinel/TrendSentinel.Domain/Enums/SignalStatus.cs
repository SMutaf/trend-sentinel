namespace TrendSentinel.Domain.Enums
{
    // Bir trend sinyalinin basit durumları
    public enum SignalStatus
    {
        /// Hala aktif olarak takip ediliyor
        Active = 0,

        /// TargetDurationDays süresi doldu
        Expired = 1,

        /// Sinyal geçersiz veya iptal edildi
        Cancelled = 2
    }
}