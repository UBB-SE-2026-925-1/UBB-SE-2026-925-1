using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PriceWatcherController : ControllerBase
{
    private readonly IPriceWatcherRepository priceWatcherRepository;

    public PriceWatcherController(IPriceWatcherRepository priceWatcherRepository)
    {
        this.priceWatcherRepository = priceWatcherRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WatchedEvent>>> GetAll()
    {
        var watched = await this.priceWatcherRepository.GetAllWatchedEventsAsync();
        return Ok(watched);
    }

    [HttpGet("{eventId}")]
    public async Task<ActionResult<WatchedEvent>> Get(int eventId)
    {
        var watched = await this.priceWatcherRepository.GetWatchAsync(eventId);
        if (watched == null) return NotFound();
        return Ok(watched);
    }

    [HttpPost]
    public async Task<ActionResult<bool>> Add([FromBody] WatchedEvent watchedEvent)
    {
        var success = await this.priceWatcherRepository.AddWatchAsync(watchedEvent);
        return Ok(success);
    }

    [HttpDelete("{eventId}")]
    public async Task<IActionResult> Remove(int eventId)
    {
        await this.priceWatcherRepository.RemoveWatchAsync(eventId);
        return NoContent();
    }

    [HttpGet("check/{eventId}")]
    public async Task<ActionResult<bool>> IsWatching(int eventId)
    {
        var isWatching = await this.priceWatcherRepository.IsWatchingAsync(eventId);
        return Ok(isWatching);
    }
}
