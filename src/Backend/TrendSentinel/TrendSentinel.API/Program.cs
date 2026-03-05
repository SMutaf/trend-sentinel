using Microsoft.EntityFrameworkCore;
using TrendSentinel.Application.Interfaces;
using TrendSentinel.Application.Mappings;
using TrendSentinel.Application.Services;
using TrendSentinel.Domain.Interfaces;
using TrendSentinel.Infrastructure.Persistence;
using TrendSentinel.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Servislerin Eklenmesi ---

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Veritabaný (PostgreSQL)
builder.Services.AddDbContext<TrendSentinelDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository (Generic)
builder.Services.AddScoped(typeof(IAsyncRepository<>), typeof(EfRepository<>));

// Servisler (Application Layer)
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<INewsLogService, NewsLogService>();
builder.Services.AddScoped<ITelegramService, TelegramService>();
builder.Services.AddScoped<IPriceHistoryService, PriceHistoryService>();
builder.Services.AddScoped<IEventTechnicalSnapshotService, EventTechnicalSnapshotService>();

// AutoMapper (MappingProfile sýnýfýný referans alarak)
builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

// --- 2. Middleware (Ýstek Hattý) ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();