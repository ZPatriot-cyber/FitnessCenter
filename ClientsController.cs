using FitnessCenter.API.Data;
using FitnessCenter.API.Models;
using FitnessCenter.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private readonly ICacheService _cache;

    public ClientsController(AppDbContext ctx, ICacheService cache)
    {
        _ctx = ctx;
        _cache = cache;
    }

    private static DateTime ToUtc(DateTime dt) =>
        dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetAll()
    {
        const string cacheKey = "clients:all";
        var cached = await _cache.GetAsync<List<ClientDto>>(cacheKey);
        if (cached != null) return Ok(cached);

        var clients = await _ctx.Clients
            .Select(c => new ClientDto(c.Id, c.FirstName, c.LastName, c.Email,
                c.Phone, c.DateOfBirth, c.MembershipStart, c.MembershipEnd, c.IsActive))
            .ToListAsync();

        await _cache.SetAsync(cacheKey, clients, TimeSpan.FromMinutes(5));
        return Ok(clients);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClientDto>> GetById(int id)
    {
        var cacheKey = $"clients:{id}";
        var cached = await _cache.GetAsync<ClientDto>(cacheKey);
        if (cached != null) return Ok(cached);

        var client = await _ctx.Clients.FindAsync(id);
        if (client == null) return NotFound();

        var dto = new ClientDto(client.Id, client.FirstName, client.LastName, client.Email,
            client.Phone, client.DateOfBirth, client.MembershipStart, client.MembershipEnd, client.IsActive);

        await _cache.SetAsync(cacheKey, dto);
        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ClientDto>> Create([FromBody] CreateClientDto dto)
    {
        var client = new Client
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            DateOfBirth = ToUtc(dto.DateOfBirth),
            MembershipStart = ToUtc(dto.MembershipStart),
            MembershipEnd = ToUtc(dto.MembershipEnd)
        };
        _ctx.Clients.Add(client);
        await _ctx.SaveChangesAsync();
        await _cache.RemoveByPatternAsync("clients:*");

        var result = new ClientDto(client.Id, client.FirstName, client.LastName, client.Email,
            client.Phone, client.DateOfBirth, client.MembershipStart, client.MembershipEnd, client.IsActive);
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClientDto dto)
    {
        var client = await _ctx.Clients.FindAsync(id);
        if (client == null) return NotFound();

        client.FirstName = dto.FirstName;
        client.LastName = dto.LastName;
        client.Email = dto.Email;
        client.Phone = dto.Phone;
        client.DateOfBirth = ToUtc(dto.DateOfBirth);
        client.MembershipStart = ToUtc(dto.MembershipStart);
        client.MembershipEnd = ToUtc(dto.MembershipEnd);
        client.IsActive = dto.IsActive;

        await _ctx.SaveChangesAsync();
        await _cache.RemoveByPatternAsync("clients:*");
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var client = await _ctx.Clients.FindAsync(id);
        if (client == null) return NotFound();

        _ctx.Clients.Remove(client);
        await _ctx.SaveChangesAsync();
        await _cache.RemoveByPatternAsync("clients:*");
        return NoContent();
    }
}
