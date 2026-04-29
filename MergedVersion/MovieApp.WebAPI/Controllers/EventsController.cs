using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventRepository eventRepository;
    private readonly IMovieRepository movieRepository;
    private readonly IFavoriteEventRepository favoriteEventRepository;
    private readonly IUserEventAttendanceRepository attendanceRepository;

    public EventsController(
        IEventRepository eventRepository,
        IMovieRepository movieRepository,
        IFavoriteEventRepository favoriteEventRepository,
        IUserEventAttendanceRepository attendanceRepository)
    {
        this.eventRepository = eventRepository;
        this.movieRepository = movieRepository;
        this.favoriteEventRepository = favoriteEventRepository;
        this.attendanceRepository = attendanceRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
    {
        var events = await this.eventRepository.GetAllAsync();
        return Ok(events);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Event>> GetEvent(int id)
    {
        var eventDetails = await this.eventRepository.FindByIdAsync(id);
        if (eventDetails == null) return NotFound();
        return Ok(eventDetails);
    }

    [HttpGet("movie/{movieId}")]
    public async Task<ActionResult<IEnumerable<Event>>> GetEventsForMovie(int movieId)
    {
        var eventIds = await this.movieRepository.FindScreeningEventIdsForMovieAsync(movieId);
        var allEvents = await this.eventRepository.GetAllAsync();
        var movieEvents = allEvents.Where(e => eventIds.Contains(e.Id));
        return Ok(movieEvents);
    }

    [HttpGet("user/{userId}/attendance")]
    public async Task<ActionResult<IEnumerable<int>>> GetAttendance(int userId)
    {
        var attendance = await this.attendanceRepository.GetJoinedEventIdsAsync(userId);
        return Ok(attendance);
    }

    [HttpPost("user/{userId}/attendance/join")]
    public async Task<IActionResult> JoinEvent(int userId, [FromQuery] int eventId)
    {
        var joinedIds = await this.attendanceRepository.GetJoinedEventIdsAsync(userId);
        if (joinedIds.Contains(eventId))
        {
            return BadRequest("User already joined this event.");
        }

        var eventDetails = await this.eventRepository.FindByIdAsync(eventId);
        if (eventDetails == null) return NotFound("Event not found.");

        if (eventDetails.CurrentEnrollment >= eventDetails.MaxCapacity)
        {
            return BadRequest("Event is full.");
        }

        await this.attendanceRepository.JoinAsync(userId, eventId);
        
        // Update enrollment count
        eventDetails.CurrentEnrollment++;
        await this.eventRepository.UpdateEventAsync(eventDetails);

        return Ok();
    }
    
    [HttpDelete("user/{userId}/attendance")]
    public async Task<IActionResult> CancelAttendance(int userId, [FromQuery] int eventId)
    {
        var joinedIds = await this.attendanceRepository.GetJoinedEventIdsAsync(userId);
        if (!joinedIds.Contains(eventId))
        {
            return BadRequest("User is not registered for this event.");
        }

        await this.attendanceRepository.CancelAttendanceAsync(userId, eventId);

        var eventDetails = await this.eventRepository.FindByIdAsync(eventId);
        if (eventDetails != null && eventDetails.CurrentEnrollment > 0)
        {
            eventDetails.CurrentEnrollment--;
            await this.eventRepository.UpdateEventAsync(eventDetails);
        }

        return Ok();
    }

    [HttpGet("favorites/{userId}")]
    public async Task<ActionResult<IEnumerable<FavoriteEvent>>> GetFavorites(int userId)
    {
        var favorites = await this.favoriteEventRepository.FindByUserAsync(userId);
        return Ok(favorites);
    }

    [HttpGet("favorites/details/{userId}")]
    public async Task<ActionResult<IEnumerable<Event>>> GetFavoriteDetails(int userId)
    {
        var favorites = await this.favoriteEventRepository.FindByUserAsync(userId);
        var allEvents = await this.eventRepository.GetAllAsync();
        var favoriteEventIds = favorites.Select(f => f.EventId).ToHashSet();
        var details = allEvents.Where(e => favoriteEventIds.Contains(e.Id));
        return Ok(details);
    }

    [HttpPost("favorites/toggle")]
    public async Task<IActionResult> ToggleFavorite([FromBody] FavoriteToggleRequest request)
    {
        var exists = await this.favoriteEventRepository.ExistsAsync(request.UserId, request.EventId);
        if (exists)
        {
            await this.favoriteEventRepository.RemoveAsync(request.UserId, request.EventId);
        }
        else
        {
            await this.favoriteEventRepository.AddAsync(request.UserId, request.EventId);
        }
        return Ok();
    }

    [HttpGet("favorites/check")]
    public async Task<ActionResult<bool>> CheckFavorite([FromQuery] int userId, [FromQuery] int eventId)
    {
        var exists = await this.favoriteEventRepository.ExistsAsync(userId, eventId);
        return Ok(exists);
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateEvent([FromBody] Event eventDetails)
    {
        var id = await this.eventRepository.AddAsync(eventDetails);
        return Ok(id);
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateEvent([FromBody] Event eventDetails)
    {
        await this.eventRepository.UpdateEventAsync(eventDetails);
        return Ok();
    }

    [HttpPost("{eventId}/enrollment")]
    public async Task<IActionResult> UpdateEnrollment(int eventId, [FromQuery] int count)
    {
        await this.eventRepository.UpdateEnrollmentAsync(eventId, count);
        return Ok();
    }

    [HttpDelete("{eventId}")]
    public async Task<IActionResult> DeleteEvent(int eventId)
    {
        await this.eventRepository.DeleteAsync(eventId);
        return Ok();
    }

    public class FavoriteToggleRequest { public int UserId { get; set; } public int EventId { get; set; } }
}
