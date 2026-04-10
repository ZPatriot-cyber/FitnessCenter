using FitnessCenter.API.Data;
using FitnessCenter.API.Models;
using FitnessCenter.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrainersController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private readonly ICacheService _cache;

    public TrainersController(AppDbContext ctx, ICacheService cache)
    {
        _ctx = ctx;
        _cache = cache;
    }

    /// <summary>Получить список всех тренеров</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrainerDto>>> GetAll()
    {
        const string cacheKey = "trainers:all";
        var cached = await _cache.GetAsync<List<TrainerDto>>(cacheKey);
        if (cached != null) return Ok(cached);

        var trainers = await _ctx.Trainers
            .Select(t => new TrainerDto(t.Id, t.FirstName, t.LastName, t.Email,
                t.Phone, t.Specialization, t.ExperienceYears, t.IsActive))
            .ToListAsync();

        await _cache.SetAsync(cacheKey, trainers, TimeSpan.FromMinutes(10));
        return Ok(trainers);
    }

    /// <summary>Получить тренера по ID</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TrainerDto>> GetById(int id)
    {
        var cacheKey = $"trainers:{id}";
        var cached = await _cache.GetAsync<TrainerDto>(cacheKey);
        if (cached != null) return Ok(cached);

        var trainer = await _ctx.Trainers.FindAsync(id);
        if (trainer == null) return NotFound();

        var dto = new TrainerDto(trainer.Id, trainer.FirstName, trainer.LastName, trainer.Email,
            trainer.Phone, trainer.Specialization, trainer.ExperienceYears, trainer.IsActive);

        await _cache.SetAsync(cacheKey, dto);
        return Ok(dto);
    }

    /// <summary>Создать тренера</summary>
    [HttpPost]
    public async Task<ActionResult<TrainerDto>> Create([FromBody] CreateTrainerDto dto)
    {
        var trainer = new Trainer
        {
            FirstName = dto.FirstName, LastName = dto.LastName, Email = dto.Email,
            Phone = dto.Phone, Specialization = dto.Specialization, ExperienceYears = dto.ExperienceYears
        };
        _ctx.Trainers.Add(trainer);
        await _ctx.SaveChangesAsync();
        await _cache.RemoveByPatternAsync("trainers:*");

        var result = new TrainerDto(trainer.Id, trainer.FirstName, trainer.LastName, trainer.Email,
            trainer.Phone, trainer.Specialization, trainer.ExperienceYears, trainer.IsActive);
        return CreatedAtAction(nameof(GetById), new { id = trainer.Id }, result);
    }

    /// <summary>Обновить тренера</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTrainerDto dto)
    {
        var trainer = await _ctx.Trainers.FindAsync(id);
        if (trainer == null) return NotFound();

        trainer.FirstName = dto.FirstName; trainer.LastName = dto.LastName;
        trainer.Email = dto.Email; trainer.Phone = dto.Phone;
        trainer.Specialization = dto.Specialization; trainer.ExperienceYears = dto.ExperienceYears;
        trainer.IsActive = dto.IsActive;

        await _ctx.SaveChangesAsync();
        await _cache.RemoveByPatternAsync("trainers:*");
        return NoContent();
    }

    /// <summary>Удалить тренера</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var trainer = await _ctx.Trainers.FindAsync(id);
        if (trainer == null) return NotFound();

        _ctx.Trainers.Remove(trainer);
        await _ctx.SaveChangesAsync();
        await _cache.RemoveByPatternAsync("trainers:*");
        return NoContent();
    }
}
