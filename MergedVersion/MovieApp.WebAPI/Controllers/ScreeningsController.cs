using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScreeningsController : ControllerBase
{
    private readonly IScreeningRepository screeningRepository;

    public ScreeningsController(IScreeningRepository screeningRepository)
    {
        this.screeningRepository = screeningRepository;
    }

    [HttpGet("event/{eventId}")]
    public async Task<ActionResult<IEnumerable<Screening>>> GetByEvent(int eventId)
    {
        var screenings = await this.screeningRepository.GetByEventIdAsync(eventId);
        return Ok(screenings);
    }

    [HttpGet("movie/{movieId}")]
    public async Task<ActionResult<IEnumerable<Screening>>> GetByMovie(int movieId)
    {
        var screenings = await this.screeningRepository.GetByMovieIdAsync(movieId);
        return Ok(screenings);
    }
}
