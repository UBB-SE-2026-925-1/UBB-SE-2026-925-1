using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using MovieApp.Infrastructure;
using MovieApp.Infrastructure.Data;

namespace MovieApp.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EventsController : ControllerBase
{
    private readonly MovieAppDbContext _context;

    public EventsController(MovieAppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
    {
        return await _context.Events.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Event>> GetEvent(int id)
    {
        var @event = await _context.Events.FindAsync(id);

        if (@event == null)
        {
            return NotFound();
        }

        return @event;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutEvent(int id, Event @event)
    {
        if (id != @event.Id)
        {
            return BadRequest();
        }

        _context.Entry(@event).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!EventExists(id))
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
    public async Task<ActionResult<Event>> PostEvent(Event @event)
    {
        _context.Events.Add(@event);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetEvent", new { id = @event.Id }, @event);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var @event = await _context.Events.FindAsync(id);
        if (@event == null)
        {
            return NotFound();
        }

        _context.Events.Remove(@event);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("movie/{movieId}")]
    public async Task<ActionResult<IEnumerable<Event>>> GetEventsForMovie(int movieId)
    {
        var eventIds = await _context.Screenings
            .Where(s => s.MovieId == movieId)
            .Select(s => s.EventId)
            .ToListAsync();

        return await _context.Events
            .Where(e => eventIds.Contains(e.Id))
            .ToListAsync();
    }

    [HttpGet("user/{userId}/attendance")]
    public async Task<ActionResult<IEnumerable<int>>> GetAttendance(int userId)
    {
        return await _context.UserEventAttendances
            .Where(a => a.UserId == userId)
            .Select(a => a.EventId)
            .ToListAsync();
    }

    [HttpPost("user/{userId}/attendance/join")]
    public async Task<IActionResult> JoinEvent(int userId, [FromQuery] int eventId)
    {
        var alreadyJoined = await _context.UserEventAttendances
            .AnyAsync(a => a.UserId == userId && a.EventId == eventId);

        if (alreadyJoined)
        {
            return BadRequest("User already joined this event.");
        }

        var eventDetails = await _context.Events.FindAsync(eventId);
        if (eventDetails == null) return NotFound("Event not found.");

        if (eventDetails.CurrentEnrollment >= eventDetails.MaxCapacity)
        {
            return BadRequest("Event is full.");
        }

        _context.UserEventAttendances.Add(new UserEventAttendance { UserId = userId, EventId = eventId });
        
        eventDetails.CurrentEnrollment++;
        _context.Entry(eventDetails).State = EntityState.Modified;

        await _context.SaveChangesAsync();

        return Ok();
    }
    
    [HttpDelete("user/{userId}/attendance")]
    public async Task<IActionResult> CancelAttendance(int userId, [FromQuery] int eventId)
    {
        var attendance = await _context.UserEventAttendances
            .FirstOrDefaultAsync(a => a.UserId == userId && a.EventId == eventId);

        if (attendance == null)
        {
            return BadRequest("User is not registered for this event.");
        }

        _context.UserEventAttendances.Remove(attendance);

        var eventDetails = await _context.Events.FindAsync(eventId);
        if (eventDetails != null && eventDetails.CurrentEnrollment > 0)
        {
            eventDetails.CurrentEnrollment--;
            _context.Entry(eventDetails).State = EntityState.Modified;
        }

        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("favorites/{userId}")]
    public async Task<ActionResult<IEnumerable<FavoriteEvent>>> GetFavorites(int userId)
    {
        return await _context.FavoriteEvents
            .Where(f => f.UserId == userId)
            .ToListAsync();
    }

    [HttpGet("favorites/details/{userId}")]
    public async Task<ActionResult<IEnumerable<Event>>> GetFavoriteDetails(int userId)
    {
        var favoriteEventIds = await _context.FavoriteEvents
            .Where(f => f.UserId == userId)
            .Select(f => f.EventId)
            .ToListAsync();

        return await _context.Events
            .Where(e => favoriteEventIds.Contains(e.Id))
            .ToListAsync();
    }

    [HttpPost("favorites/toggle")]
    public async Task<IActionResult> ToggleFavorite([FromBody] FavoriteToggleRequest request)
    {
        var favorite = await _context.FavoriteEvents
            .FirstOrDefaultAsync(f => f.UserId == request.UserId && f.EventId == request.EventId);

        if (favorite != null)
        {
            _context.FavoriteEvents.Remove(favorite);
        }
        else
        {
            _context.FavoriteEvents.Add(new FavoriteEvent { UserId = request.UserId, EventId = request.EventId });
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("favorites/check")]
    public async Task<ActionResult<bool>> CheckFavorite([FromQuery] int userId, [FromQuery] int eventId)
    {
        return await _context.FavoriteEvents.AnyAsync(f => f.UserId == userId && f.EventId == eventId);
    }

    [HttpPost("update-enrollment")]
    public async Task<IActionResult> UpdateEnrollment(int eventId, [FromQuery] int count)
    {
        var eventDetails = await _context.Events.FindAsync(eventId);
        if (eventDetails == null) return NotFound();

        eventDetails.CurrentEnrollment = count;
        _context.Entry(eventDetails).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return Ok();
    }

    private bool EventExists(int id)
    {
        return _context.Events.Any(e => e.Id == id);
    }

    public class FavoriteToggleRequest { public int UserId { get; set; } public int EventId { get; set; } }
}
