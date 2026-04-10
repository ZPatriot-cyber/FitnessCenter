using FitnessCenter.API.Data;
using FitnessCenter.API.Services;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var connStr = Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? "Host=localhost;Port=5432;Database=fitnessdb;Username=fitness;Password=fitness123";

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connStr));

var redisConn = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConn));
builder.Services.AddScoped<ICacheService, RedisCacheService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "FitnessCenter API", Version = "v1", Description = "Вариант 19 - Система управления фитнес-центром" });
});

builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// EnsureCreated создаёт таблицы напрямую без миграций
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await AppDbContext.SeedAsync(db);
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FitnessCenter API v1"));
app.UseHttpMetrics();
app.UseRouting();
app.UseCors();
app.MapControllers();
app.MapMetrics();

app.Run();
