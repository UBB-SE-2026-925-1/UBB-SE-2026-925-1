using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.Core.DTOs
{
    public sealed class BadgeDTO
    {
        public int BadgeId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int CriteriaValue { get; set; }
    }

}