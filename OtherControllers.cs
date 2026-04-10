using FitnessCenter.API.Data;
using FitnessCenter.API.Models;
using FitnessCenter.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FitnessClassesController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private readonly ICacheService _cache;

    public FitnessClassesController(AppDbContext ctx, ICacheService cache)
    {
        _ctx = ctx; _cache = cache;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FitnessClassDto>>> GetAll()
    {
        const string cacheKey = "classes:all";
        var cached = await _cache.GetAsync<List<FitnessClassDto>>(cacheKey);
        if (cached != null) return Ok(cached);

        var classes = await _ctx.FitnessClasses.Include(c => c.Trainer)
            .Select(c => new FitnessClassDto(c.Id, c.Name, c.Description, c.DurationMinutes,
                c.MaxParticipants, c.Difficulty, c.TrainerId,
                c.Trainer != null ? c.Trainer.FirstName + " " + c.Trainer.LastName : null))
            .ToListAsync();

        await _cache.SetAsync(cacheKey, classes, TimeSpan.FromMinutes(10));
        return Ok(classes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FitnessClassDto>> GetById(int id)
    {
        var cls = await _ctx.FitnessClasses.Include(c => c.Trainer).FirstOrDefaultAsync(c => c.Id == id);
        if (cls == null) return NotFound();
        return Ok(new FitnessClassDto(cls.Id, cls.Name, cls.Description, cls.DurationMinutes,
            cls.MaxParticipants, cls.Difficulty, cls.TrainerId,
            cls.Trainer != null ? cls.Trainer.FirstName + " " + cls.Trainer.LastName : null));
    }

    [HttpPost]
    public async Task<ActionResult<FitnessClassDto>> Create([FromBody] CreateFitnessClassDto dto)
    {
        var cls = new FitnessClass
        {
            Name = dto.Name, Description = dto.Description, DurationMinutes = dto.DurationMinutes,
            MaxParticipants = dto.MaxParticipants, Difficulty = dto.Difficulty, TrainerId = dto.TrainerId
        };
        _ctx.FitnessClasses.Add(cls);
        await _ctx.SaveChangesAsync();
        await _cache.RemoveByPatternAsync("classes:*");
        return CreatedAtAction(nameof(GetById), new { id = cls.Id },
            new FitnessClassDto(cls.Id, cls.Name, cls.Description, cls.DurationMinutes,
                cls.MaxParticipants, cls.Difficulty, cls.TrainerId, null));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFitnessClassDto dto)
    {
        var cls = await _ctx.FitnessClasses.FindAsync(id);
        if (cls == null) return NotFound();
        cls.Name = dto.Name; cls.Description = dto.Description;
        cls.DurationMinutes = dto.DurationMinutes; cls.MaxParticipants = dto.MaxParticipants;
        cls.Difficulty = dto.Difficulty; cls.TrainerId = dto.TrainerId;
        await _ctx.SaveChangesAsync();
        await _cache.RemoveByPatternAsync("classes:*");
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cls = await _ctx.FitnessClasses.FindAsync(id);
        if (cls == null) return NotFound();
        _ctx.FitnessClasses.Remove(cls);
        await _ctx.SaveChangesAsync();
        await _cache.RemoveByPatternAsync("classes:*");
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
public class SchedulesController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private readonly ICacheService _cache;

    public SchedulesController(AppDbContext ctx, ICacheService cache)
    {
        _ctx = ctx; _cache = cache;
    }

    private static DateTime ToUtc(DateTime dt) =>
        dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScheduleDto>>> GetAll()
    {
        var schedules = await _ctx.Schedules
            .Include(s => s.FitnessClass)
            .Include(s => s.Client)
            .Select(s => new ScheduleDto(s.Id, s.ClassId,
                s.FitnessClass != null ? s.FitnessClass.Name : null,
                s.ClientId,
                s.Client != null ? s.Client.FirstName + " " + s.Client.LastName : null,
                s.StartTime, s.Status, s.Notes))
            .ToListAsync();
        return Ok(schedules);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ScheduleDto>> GetById(int id)
    {
        var s = await _ctx.Schedules.Include(x => x.FitnessClass).Include(x => x.Client)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (s == null) return NotFound();
        return Ok(new ScheduleDto(s.Id, s.ClassId,
            s.FitnessClass?.Name, s.ClientId,
            s.Client != null ? s.Client.FirstName + " " + s.Client.LastName : null,
            s.StartTime, s.Status, s.Notes));
    }

    [HttpGet("client/{clientId}")]
    public async Task<ActionResult<IEnumerable<ScheduleDto>>> GetByClient(int clientId)
    {
        var cacheKey = $"schedules:client:{clientId}";
        var cached = await _cache.GetAsync<List<ScheduleDto>>(cacheKey);
        if (cached != null) return Ok(cached);

        var schedules = await _ctx.Schedules
            .Where(s => s.ClientId == clientId)
            .Include(s => s.FitnessClass)
            .Include(s => s.Client)
            .Select(s => new ScheduleDto(s.Id, s.ClassId,
                s.FitnessClass != null ? s.FitnessClass.Name : null,
                s.ClientId,
                s.Client != null ? s.Client.FirstName + " " + s.Client.LastName : null,
                s.StartTime, s.Status, s.Notes))
            .ToListAsync();

        await _cache.SetAsync(cacheKey, schedules, TimeSpan.FromMinutes(3));
        return Ok(schedules);
    }

    [HttpPost]
    public async Task<ActionResult<ScheduleDto>> Create([FromBody] CreateScheduleDto dto)
    {
        var s = new Schedule
        {
            ClassId = dto.ClassId,
            ClientId = dto.ClientId,
            StartTime = ToUtc(dto.StartTime),
            Notes = dto.Notes
        };
        _ctx.Schedules.Add(s);
        await _ctx.SaveChangesAsync();
        await _cache.RemoveByPatternAsync("schedules:*");
        return CreatedAtAction(nameof(GetById), new { id = s.Id },
            new ScheduleDto(s.Id, s.ClassId, null, s.ClientId, null, s.StartTime, s.Status, s.Notes));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateScheduleDto dto)
    {
        var s = await _ctx.Schedules.FindAsync(id);
        if (s == null) return NotFound();
        s.ClassId = dto.ClassId;
        s.ClientId = dto.ClientId;
        s.StartTime = ToUtc(dto.StartTime);
        s.Status = dto.Status;
        s.Notes = dto.Notes;
        await _ctx.SaveChangesAsync();
        await _cache.RemoveByPatternAsync("schedules:*");
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await _ctx.Schedules.FindAsync(id);
        if (s == null) return NotFound();
        _ctx.Schedules.Remove(s);
        await _ctx.SaveChangesAsync();
        await _cache.RemoveByPatternAsync("schedules:*");
        return NoContent();
    }
}
