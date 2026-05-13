using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;

namespace MovieApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BadgesController : ControllerBase
{
    private readonly IBadgeService badgeService;

    public BadgesController(IBadgeService badgeService)
    {
        this.badgeService = badgeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Badge>>> GetAll()
    {
        var badges = await this.badgeService.GetAllBadgesAsync();
        return Ok(badges);
    }
}
