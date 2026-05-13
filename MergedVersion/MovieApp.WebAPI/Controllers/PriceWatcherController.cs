using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using MovieApp.Infrastructure;
using MovieApp.Infrastructure.Data;

namespace MovieApp.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PriceWatcherController : ControllerBase
{
    private readonly MovieAppDbContext _context;

    public PriceWatcherController(MovieAppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WatchedEvent>>> GetWatchedEvents()
    {
        return await _context.WatchedEvents.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WatchedEvent>> GetWatchedEvent(int id)
    {
        var watchedEvent = await _context.WatchedEvents.FindAsync(id);

        if (watchedEvent == null)
        {
            return NotFound();
        }

        return watchedEvent;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutWatchedEvent(int id, WatchedEvent watchedEvent)
    {
        if (id != watchedEvent.EventId)
        {
            return BadRequest();
        }

        _context.Entry(watchedEvent).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!WatchedEventExists(id))
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
    public async Task<ActionResult<WatchedEvent>> PostWatchedEvent(WatchedEvent watchedEvent)
    {
        _context.WatchedEvents.Add(watchedEvent);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetWatchedEvent", new { id = watchedEvent.EventId }, watchedEvent);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWatchedEvent(int id)
    {
        var watchedEvent = await _context.WatchedEvents.FindAsync(id);
        if (watchedEvent == null)
        {
            return NotFound();
        }

        _context.WatchedEvents.Remove(watchedEvent);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("check/{eventId}")]
    public async Task<ActionResult<bool>> IsWatching(int eventId)
    {
        return await _context.WatchedEvents.AnyAsync(w => w.EventId == eventId);
    }

    private bool WatchedEventExists(int id)
    {
        return _context.WatchedEvents.Any(e => e.EventId == id);
    }
}
