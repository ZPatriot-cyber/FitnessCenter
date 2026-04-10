using FitnessCenter.API.Data;
using FitnessCenter.API.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FitnessCenter.Tests;

public class DbContextTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CanAddAndRetrieveClient()
    {
        using var ctx = CreateInMemoryContext();
        var client = new Client
        {
            FirstName = "Иван", LastName = "Тестов", Email = "test@test.ru",
            Phone = "+7-000-000-0000", DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            MembershipStart = DateTime.UtcNow, MembershipEnd = DateTime.UtcNow.AddMonths(12)
        };
        ctx.Clients.Add(client);
        await ctx.SaveChangesAsync();

        var retrieved = await ctx.Clients.FirstOrDefaultAsync(c => c.Email == "test@test.ru");
        Assert.NotNull(retrieved);
        Assert.Equal("Иван", retrieved.FirstName);
    }

    [Fact]
    public async Task CanAddAndRetrieveTrainer()
    {
        using var ctx = CreateInMemoryContext();
        var trainer = new Trainer
        {
            FirstName = "Алекс", LastName = "Тренеров", Email = "trainer@test.ru",
            Phone = "+7-111-111-1111", Specialization = "Йога", ExperienceYears = 5
        };
        ctx.Trainers.Add(trainer);
        await ctx.SaveChangesAsync();

        var count = await ctx.Trainers.CountAsync();
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task CanCreateFitnessClassLinkedToTrainer()
    {
        using var ctx = CreateInMemoryContext();
        var trainer = new Trainer
        {
            FirstName = "Марина", LastName = "Силова", Email = "marina@test.ru",
            Phone = "+7-222-222-2222", Specialization = "Пилатес", ExperienceYears = 3
        };
        ctx.Trainers.Add(trainer);
        await ctx.SaveChangesAsync();

        var cls = new FitnessClass
        {
            Name = "Пилатес базовый", Description = "Для начинающих",
            DurationMinutes = 45, MaxParticipants = 10, Difficulty = "Beginner",
            TrainerId = trainer.Id
        };
        ctx.FitnessClasses.Add(cls);
        await ctx.SaveChangesAsync();

        var loaded = await ctx.FitnessClasses.Include(c => c.Trainer).FirstAsync();
        Assert.Equal("Марина", loaded.Trainer!.FirstName);
        Assert.Equal("Пилатес базовый", loaded.Name);
    }

    [Fact]
    public async Task CanCreateScheduleForClientAndClass()
    {
        using var ctx = CreateInMemoryContext();
        var trainer = new Trainer { FirstName = "T", LastName = "T", Email = "t@t.ru", Phone = "0", Specialization = "X", ExperienceYears = 1 };
        ctx.Trainers.Add(trainer);
        await ctx.SaveChangesAsync();

        var cls = new FitnessClass { Name = "Тест", Description = "D", DurationMinutes = 30, MaxParticipants = 5, Difficulty = "Beginner", TrainerId = trainer.Id };
        var client = new Client { FirstName = "C", LastName = "C", Email = "c@c.ru", Phone = "0", DateOfBirth = DateTime.UtcNow.AddYears(-25), MembershipStart = DateTime.UtcNow, MembershipEnd = DateTime.UtcNow.AddMonths(1) };
        ctx.FitnessClasses.Add(cls);
        ctx.Clients.Add(client);
        await ctx.SaveChangesAsync();

        var schedule = new Schedule { ClassId = cls.Id, ClientId = client.Id, StartTime = DateTime.UtcNow.AddDays(1) };
        ctx.Schedules.Add(schedule);
        await ctx.SaveChangesAsync();

        var count = await ctx.Schedules.CountAsync();
        Assert.Equal(1, count);
        Assert.Equal("Registered", schedule.Status);
    }

    [Fact]
    public async Task CanDeleteClient()
    {
        using var ctx = CreateInMemoryContext();
        var client = new Client
        {
            FirstName = "Delete", LastName = "Me", Email = "del@test.ru", Phone = "0",
            DateOfBirth = DateTime.UtcNow.AddYears(-20), MembershipStart = DateTime.UtcNow, MembershipEnd = DateTime.UtcNow.AddMonths(1)
        };
        ctx.Clients.Add(client);
        await ctx.SaveChangesAsync();

        ctx.Clients.Remove(client);
        await ctx.SaveChangesAsync();

        var count = await ctx.Clients.CountAsync();
        Assert.Equal(0, count);
    }
}
