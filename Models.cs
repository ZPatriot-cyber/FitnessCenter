namespace FitnessCenter.API.Models;

public class Client
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime MembershipStart { get; set; }
    public DateTime MembershipEnd { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}

public class Trainer
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<FitnessClass> Classes { get; set; } = new List<FitnessClass>();
}

public class FitnessClass
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public int MaxParticipants { get; set; }
    public string Difficulty { get; set; } = string.Empty; // Beginner, Intermediate, Advanced
    public int TrainerId { get; set; }
    public Trainer? Trainer { get; set; }

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}

public class Schedule
{
    public int Id { get; set; }
    public int ClassId { get; set; }
    public FitnessClass? FitnessClass { get; set; }
    public int ClientId { get; set; }
    public Client? Client { get; set; }
    public DateTime StartTime { get; set; }
    public string Status { get; set; } = "Registered"; // Registered, Completed, Cancelled
    public string? Notes { get; set; }
}
