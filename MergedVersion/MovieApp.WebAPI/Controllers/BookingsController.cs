using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Infrastructure;
using MovieApp.Infrastructure.Data;

namespace MovieApp.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookingsController : ControllerBase
{
    private readonly MovieAppDbContext _context;
    private readonly IBookingRepository _bookingRepository;

    public BookingsController(MovieAppDbContext context, IBookingRepository bookingRepository)
    {
        _context = context;
        _bookingRepository = bookingRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SeatBooking>>> GetSeatBookings()
    {
        return await _context.SeatBookings.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SeatBooking>> GetSeatBooking(int id)
    {
        var seatBooking = await _context.SeatBookings.FindAsync(id);

        if (seatBooking == null)
        {
            return NotFound();
        }

        return seatBooking;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutSeatBooking(int id, SeatBooking seatBooking)
    {
        if (id != seatBooking.Id)
        {
            return BadRequest();
        }

        _context.Entry(seatBooking).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!SeatBookingExists(id))
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
    public async Task<ActionResult<SeatBooking>> PostSeatBooking(SeatBooking seatBooking)
    {
        _context.SeatBookings.Add(seatBooking);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetSeatBooking", new { id = seatBooking.Id }, seatBooking);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSeatBooking(int id)
    {
        var seatBooking = await _context.SeatBookings.FindAsync(id);
        if (seatBooking == null)
        {
            return NotFound();
        }

        _context.SeatBookings.Remove(seatBooking);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("screening/{screeningId}")]
    public async Task<ActionResult<IEnumerable<SeatBooking>>> GetByScreening(int screeningId)
    {
        return await _context.SeatBookings
            .Where(b => b.ScreeningId == screeningId)
            .ToListAsync();
    }

    [HttpPost("reserve")]
    public async Task<ActionResult<bool>> Reserve([FromBody] ReserveRequest request)
    {
        if (request is null || request.Seats is null || request.Seats.Count == 0)
        {
            return BadRequest("Seats are required.");
        }

        var seats = request.Seats.Select(s => (s.Row, s.Column)).ToList();
        var ok = await _bookingRepository.ReserveAsync(request.ScreeningId, request.UserId, seats);
        return Ok(ok);
    }

    private bool SeatBookingExists(int id)
    {
        return _context.SeatBookings.Any(e => e.Id == id);
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
}
