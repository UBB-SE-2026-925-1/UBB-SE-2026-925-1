using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingRepository bookingRepository;

    public BookingsController(IBookingRepository bookingRepository)
    {
        this.bookingRepository = bookingRepository;
    }

    public sealed class SeatDto
    {
        public int Row { get; set; }
        public int Column { get; set; }
    }

    public sealed class ReserveRequest
    {
        public int ScreeningId { get; set; }
        public int UserId { get; set; }
        public List<SeatDto> Seats { get; set; } = new();
    }

    [HttpGet("screening/{screeningId}")]
    public async Task<ActionResult<IEnumerable<SeatBooking>>> GetByScreening(int screeningId)
    {
        var bookings = await this.bookingRepository.GetByScreeningAsync(screeningId);
        return Ok(bookings);
    }

    [HttpPost("reserve")]
    public async Task<ActionResult<bool>> Reserve([FromBody] ReserveRequest request)
    {
        if (request is null || request.Seats is null || request.Seats.Count == 0)
        {
            return BadRequest("Seats are required.");
        }

        var seats = request.Seats.Select(s => (s.Row, s.Column)).ToList();
        var ok = await this.bookingRepository.ReserveAsync(request.ScreeningId, request.UserId, seats);
        return Ok(ok);
    }
}
