using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using MovieApp.Infrastructure;
using MovieApp.Infrastructure.Data;

namespace MovieApp.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ScreeningsController : ControllerBase
{
    private readonly MovieAppDbContext _context;

    public ScreeningsController(MovieAppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Screening>>> GetScreenings()
    {
        return await _context.Screenings.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Screening>> GetScreening(int id)
    {
        var screening = await _context.Screenings.FindAsync(id);

        if (screening == null)
        {
            return NotFound();
        }

        return screening;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutScreening(int id, Screening screening)
    {
        if (id != screening.Id)
        {
            return BadRequest();
        }

        _context.Entry(screening).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ScreeningExists(id))
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
    public async Task<ActionResult<Screening>> PostScreening(Screening screening)
    {
        _context.Screenings.Add(screening);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetScreening", new { id = screening.Id }, screening);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteScreening(int id)
    {
        var screening = await _context.Screenings.FindAsync(id);
        if (screening == null)
        {
            return NotFound();
        }

        _context.Screenings.Remove(screening);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("event/{eventId}")]
    public async Task<ActionResult<IEnumerable<Screening>>> GetByEvent(int eventId)
    {
        return await _context.Screenings
            .Where(s => s.EventId == eventId)
            .ToListAsync();
    }

    [HttpGet("movie/{movieId}")]
    public async Task<ActionResult<IEnumerable<Screening>>> GetByMovie(int movieId)
    {
        return await _context.Screenings
            .Where(s => s.MovieId == movieId)
            .ToListAsync();
    }

    private bool ScreeningExists(int id)
    {
        return _context.Screenings.Any(e => e.Id == id);
    }
}
