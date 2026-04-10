using FitnessCenter.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenter.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Trainer> Trainers => Set<Trainer>();
    public DbSet<FitnessClass> FitnessClasses => Set<FitnessClass>();
    public DbSet<Schedule> Schedules => Set<Schedule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>().HasIndex(c => c.Email).IsUnique();
        modelBuilder.Entity<Trainer>().HasIndex(t => t.Email).IsUnique();

        modelBuilder.Entity<FitnessClass>()
            .HasOne(fc => fc.Trainer)
            .WithMany(t => t.Classes)
            .HasForeignKey(fc => fc.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Schedule>()
            .HasOne(s => s.FitnessClass)
            .WithMany(fc => fc.Schedules)
            .HasForeignKey(s => s.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Schedule>()
            .HasOne(s => s.Client)
            .WithMany(c => c.Schedules)
            .HasForeignKey(s => s.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static DateTime Utc(int year, int month, int day) =>
        DateTime.SpecifyKind(new DateTime(year, month, day), DateTimeKind.Utc);

    private static DateTime UtcNowPlus(int months) =>
        DateTime.SpecifyKind(DateTime.UtcNow.AddMonths(months), DateTimeKind.Utc);

    private static DateTime UtcNowPlusDays(int days, int hours = 0) =>
        DateTime.SpecifyKind(DateTime.UtcNow.AddDays(days).Date.AddHours(hours), DateTimeKind.Utc);

    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Trainers.AnyAsync()) return;

        var trainers = new List<Trainer>
        {
            new() { FirstName = "Алексей", LastName = "Иванов", Email = "ivanov@fitness.ru", Phone = "+7-900-111-1111", Specialization = "Силовые тренировки", ExperienceYears = 7 },
            new() { FirstName = "Мария",   LastName = "Петрова", Email = "petrova@fitness.ru", Phone = "+7-900-222-2222", Specialization = "Йога и растяжка",    ExperienceYears = 5 },
            new() { FirstName = "Дмитрий", LastName = "Сидоров", Email = "sidorov@fitness.ru", Phone = "+7-900-333-3333", Specialization = "Кардио и HIIT",       ExperienceYears = 4 },
        };
        context.Trainers.AddRange(trainers);
        await context.SaveChangesAsync();

        var classes = new List<FitnessClass>
        {
            new() { Name = "Силовой тренинг",  Description = "Тренировка с отягощениями для наращивания мышечной массы", DurationMinutes = 60, MaxParticipants = 10, Difficulty = "Intermediate", TrainerId = trainers[0].Id },
            new() { Name = "Утренняя йога",    Description = "Мягкие упражнения для гибкости и расслабления",            DurationMinutes = 45, MaxParticipants = 15, Difficulty = "Beginner",      TrainerId = trainers[1].Id },
            new() { Name = "HIIT",             Description = "Высокоинтенсивные интервальные тренировки для сжигания жира", DurationMinutes = 30, MaxParticipants = 12, Difficulty = "Advanced",   TrainerId = trainers[2].Id },
            new() { Name = "Пилатес",          Description = "Упражнения для укрепления кора и улучшения осанки",         DurationMinutes = 50, MaxParticipants = 10, Difficulty = "Beginner",      TrainerId = trainers[1].Id },
        };
        context.FitnessClasses.AddRange(classes);
        await context.SaveChangesAsync();

        var clients = new List<Client>
        {
            new() { FirstName = "Анна",   LastName = "Козлова",  Email = "kozlova@mail.ru",  Phone = "+7-900-444-4444", DateOfBirth = Utc(1990, 3, 15),  MembershipStart = UtcNowPlus(-2), MembershipEnd = UtcNowPlus(10) },
            new() { FirstName = "Игорь",  LastName = "Новиков",  Email = "novikov@mail.ru",  Phone = "+7-900-555-5555", DateOfBirth = Utc(1985, 7, 22),  MembershipStart = UtcNowPlus(-1), MembershipEnd = UtcNowPlus(11) },
            new() { FirstName = "Елена",  LastName = "Морозова", Email = "morozova@mail.ru", Phone = "+7-900-666-6666", DateOfBirth = Utc(1995, 11, 5),  MembershipStart = DateTime.UtcNow, MembershipEnd = UtcNowPlus(12) },
            new() { FirstName = "Сергей", LastName = "Волков",   Email = "volkov@mail.ru",   Phone = "+7-900-777-7777", DateOfBirth = Utc(1988, 1, 30),  MembershipStart = UtcNowPlus(-3), MembershipEnd = UtcNowPlus(9) },
        };
        context.Clients.AddRange(clients);
        await context.SaveChangesAsync();

        var schedules = new List<Schedule>
        {
            new() { ClassId = classes[0].Id, ClientId = clients[0].Id, StartTime = UtcNowPlusDays(1, 10),  Status = "Registered" },
            new() { ClassId = classes[1].Id, ClientId = clients[0].Id, StartTime = UtcNowPlusDays(2, 9),   Status = "Registered" },
            new() { ClassId = classes[2].Id, ClientId = clients[1].Id, StartTime = UtcNowPlusDays(1, 18),  Status = "Registered" },
            new() { ClassId = classes[3].Id, ClientId = clients[2].Id, StartTime = UtcNowPlusDays(3, 11),  Status = "Registered" },
            new() { ClassId = classes[0].Id, ClientId = clients[3].Id, StartTime = UtcNowPlusDays(-1, 10), Status = "Completed"  },
        };
        context.Schedules.AddRange(schedules);
        await context.SaveChangesAsync();
    }
}
