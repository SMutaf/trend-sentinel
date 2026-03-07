using Microsoft.EntityFrameworkCore;
using TrendSentinel.Domain.Entities;
using TrendSentinel.Domain.ValueObjects;

namespace TrendSentinel.Infrastructure.Persistence
{
    public class TrendSentinelDbContext : DbContext
    {
        public TrendSentinelDbContext(DbContextOptions<TrendSentinelDbContext> options) : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<NewsLog> NewsLogs { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }
        public DbSet<EventTechnicalSnapshot> EventTechnicalSnapshots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Company Konfigürasyonu
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
                entity.Property(c => c.TickerSymbol).IsRequired().HasMaxLength(10);
            });

            // NewsLog Konfigürasyonu
            modelBuilder.Entity<NewsLog>(entity =>
            {
                entity.HasKey(n => n.Id);

                // Value Objects - Owned Types (Aynı tablo içinde saklanır)
                entity.OwnsOne(n => n.Analysis, a =>
                {
                    a.Property(x => x.IsTrendTriggered).HasColumnName("Analysis_IsTrendTriggered");
                    a.Property(x => x.TrendSummary).HasColumnName("Analysis_TrendSummary").HasMaxLength(1000);
                    a.Property(x => x.Sentiment).HasColumnName("Analysis_Sentiment").HasConversion<string>();
                    a.Property(x => x.ConfidenceScore).HasColumnName("Analysis_ConfidenceScore");
                });

                entity.OwnsOne(n => n.QuantMetrics, q =>
                {
                    q.Property(x => x.EventType).HasColumnName("Quant_EventType").HasConversion<string>();
                    q.Property(x => x.ImpactStrength).HasColumnName("Quant_ImpactStrength");
                    q.Property(x => x.ExpectedDirection).HasColumnName("Quant_ExpectedDirection").HasConversion<string>();
                    q.Property(x => x.TimeHorizon).HasColumnName("Quant_TimeHorizon").HasConversion<string>();
                    q.Property(x => x.OverextendedRisk).HasColumnName("Quant_OverextendedRisk");
                });

                // 1:1 İlişkiler
                entity.HasOne(n => n.PriceSnapshot)
                      .WithOne(p => p.NewsLog)
                      .HasForeignKey<PriceHistory>(p => p.NewsLogId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(n => n.TechnicalSnapshot)
                      .WithOne(t => t.NewsLog)
                      .HasForeignKey<EventTechnicalSnapshot>(t => t.NewsLogId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Company ile ilişki
                entity.HasOne(n => n.Company)
                      .WithMany(c => c.NewsLogs)
                      .HasForeignKey(n => n.CompanyId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // PriceHistory Konfigürasyonu
            modelBuilder.Entity<PriceHistory>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.NewsLogId).IsUnique();
            });

            // EventTechnicalSnapshot Konfigürasyonu
            modelBuilder.Entity<EventTechnicalSnapshot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.NewsLogId).IsUnique();
            });
        }
    }
}