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

            // --- İlişki Ayarları (One-to-Many) ---
            // Bir Şirketin (Company) birden çok Haberi (NewsLogs) olabilir.
            modelBuilder.Entity<Company>()
                .HasMany(c => c.NewsLogs)
                .WithOne(n => n.Company)
                .HasForeignKey(n => n.CompanyId)
                .OnDelete(DeleteBehavior.Cascade); // Şirket silinirse haberleri de silinsin.

            base.OnModelCreating(modelBuilder);
        }
    }
}