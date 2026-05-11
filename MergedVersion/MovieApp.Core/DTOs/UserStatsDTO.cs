using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.Core.DTOs
{
    public sealed class UserStatsDTO
    {
        public int UserId { get; set; }
        public int TotalPoints { get; set; }

        public int WeeklyScore { get; set; }
    }
}
