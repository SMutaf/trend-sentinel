using Microsoft.EntityFrameworkCore;
using TrendSentinel.Domain.Entities;

namespace TrendSentinel.Infrastructure.Persistence
{
    public class TrendSentinelDbContext : DbContext
    {
        public TrendSentinelDbContext(DbContextOptions<TrendSentinelDbContext> options) : base(options)
        {
        }

        // Tablolarımız
        public DbSet<Company> Companies { get; set; }
        public DbSet<NewsLog> NewsLogs { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }
        public DbSet<EventTechnicalSnapshot> EventTechnicalSnapshots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // --- Company Ayarları ---
            modelBuilder.Entity<Company>()
                .HasKey(c => c.Id);
            modelBuilder.Entity<Company>()
                .Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(200);
            modelBuilder.Entity<Company>()
                .Property(c => c.TickerSymbol)
                .IsRequired()
                .HasMaxLength(10);

            // --- Company -> NewsLogs (1:Many) ---
            modelBuilder.Entity<Company>()
                .HasMany(c => c.NewsLogs)
                .WithOne(n => n.Company)
                .HasForeignKey(n => n.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // === PriceHistory: 1:1 PriceSnapshot (DEĞİŞMEDİ - HABER ANI FİYATI) ===
            modelBuilder.Entity<NewsLog>()
                .HasOne(n => n.PriceSnapshot)
                .WithOne(p => p.NewsLog)
                .HasForeignKey<PriceHistory>(p => p.NewsLogId)
                .OnDelete(DeleteBehavior.Cascade);

            // === EventTechnicalSnapshot: 1:1 (DEĞİŞMEDİ) ===
            modelBuilder.Entity<NewsLog>()
                .HasOne(n => n.TechnicalSnapshot)
                .WithOne(t => t.NewsLog)
                .HasForeignKey<EventTechnicalSnapshot>(t => t.NewsLogId)
                .OnDelete(DeleteBehavior.Cascade);

            // === YENİ: NewsLog -> SignalTrack (1:0..1) ===
            modelBuilder.Entity<NewsLog>()
                .HasOne(n => n.SignalTrack)
                .WithOne(s => s.NewsLog)
                .HasForeignKey<SignalTrack>(s => s.NewsLogId)
                .OnDelete(DeleteBehavior.Cascade);

            // === YENİ: SignalTrack -> SignalPricePoint (1:Many) ===
            modelBuilder.Entity<SignalTrack>()
                .HasMany(s => s.PricePoints)
                .WithOne(p => p.SignalTrack)
                .HasForeignKey(p => p.SignalTrackId)
                .OnDelete(DeleteBehavior.Cascade);

            // === YENİ: Performance Index'leri ===
            modelBuilder.Entity<SignalTrack>()
                .HasIndex(s => new { s.Status, s.EntryDate });

            modelBuilder.Entity<SignalTrack>()
                .HasIndex(s => s.CompanyId);

            modelBuilder.Entity<SignalPricePoint>()
                .HasIndex(p => new { p.SignalTrackId, p.DayNumber })
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}