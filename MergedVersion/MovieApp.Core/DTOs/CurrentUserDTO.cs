using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.Core.DTOs
{
    public sealed class CurrentUserDTO
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public int TotalPoints { get; set; }

        public int WeeklyScore { get; set; }
    }
}
