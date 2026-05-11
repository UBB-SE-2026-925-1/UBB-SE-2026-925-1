using MovieApp.Core.DTOs;
using MovieApp.Core.Models;

namespace MovieApp.WebAPI.Controllers.DTOs
{
    public sealed class UserBadgesDTO
    {
        public int UserId { get; set; }
        public List<BadgeDTO> Badges { get; set; } = new();
    }
}
