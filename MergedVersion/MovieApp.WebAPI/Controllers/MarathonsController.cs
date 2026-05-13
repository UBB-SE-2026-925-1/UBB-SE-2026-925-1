using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using MovieApp.Infrastructure;
using MovieApp.Infrastructure.Data;

namespace MovieApp.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MarathonsController : ControllerBase
{
    private readonly MovieAppDbContext _context;

    public MarathonsController(MovieAppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Marathon>>> GetMarathons()
    {
        return await _context.Marathons.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Marathon>> GetMarathon(int id)
    {
        var marathon = await _context.Marathons.FindAsync(id);

        if (marathon == null)
        {
            return NotFound();
        }

        return marathon;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutMarathon(int id, Marathon marathon)
    {
        if (id != marathon.Id)
        {
            return BadRequest();
        }

        _context.Entry(marathon).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MarathonExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<Marathon>> PostMarathon(Marathon marathon)
    {
        _context.Marathons.Add(marathon);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetMarathon", new { id = marathon.Id }, marathon);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMarathon(int id)
    {
        var marathon = await _context.Marathons.FindAsync(id);
        if (marathon == null)
        {
            return NotFound();
        }

        _context.Marathons.Remove(marathon);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool MarathonExists(int id)
    {
        return _context.Marathons.Any(e => e.Id == id);
    }
}