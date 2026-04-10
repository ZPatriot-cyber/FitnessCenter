namespace FitnessCenter.API.Models;

// Client DTOs
public record ClientDto(int Id, string FirstName, string LastName, string Email,
    string Phone, DateTime DateOfBirth, DateTime MembershipStart, DateTime MembershipEnd, bool IsActive);

public record CreateClientDto(string FirstName, string LastName, string Email,
    string Phone, DateTime DateOfBirth, DateTime MembershipStart, DateTime MembershipEnd);

public record UpdateClientDto(string FirstName, string LastName, string Email,
    string Phone, DateTime DateOfBirth, DateTime MembershipStart, DateTime MembershipEnd, bool IsActive);

// Trainer DTOs
public record TrainerDto(int Id, string FirstName, string LastName, string Email,
    string Phone, string Specialization, int ExperienceYears, bool IsActive);

public record CreateTrainerDto(string FirstName, string LastName, string Email,
    string Phone, string Specialization, int ExperienceYears);

public record UpdateTrainerDto(string FirstName, string LastName, string Email,
    string Phone, string Specialization, int ExperienceYears, bool IsActive);

// FitnessClass DTOs
public record FitnessClassDto(int Id, string Name, string Description, int DurationMinutes,
    int MaxParticipants, string Difficulty, int TrainerId, string? TrainerName);

public record CreateFitnessClassDto(string Name, string Description, int DurationMinutes,
    int MaxParticipants, string Difficulty, int TrainerId);

public record UpdateFitnessClassDto(string Name, string Description, int DurationMinutes,
    int MaxParticipants, string Difficulty, int TrainerId);

// Schedule DTOs
public record ScheduleDto(int Id, int ClassId, string? ClassName, int ClientId,
    string? ClientName, DateTime StartTime, string Status, string? Notes);

public record CreateScheduleDto(int ClassId, int ClientId, DateTime StartTime, string? Notes);

public record UpdateScheduleDto(int ClassId, int ClientId, DateTime StartTime, string Status, string? Notes);
