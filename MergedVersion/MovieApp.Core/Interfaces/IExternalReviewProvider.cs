using System.Threading;
using System.Threading.Tasks;
#nullable enable
using MovieApp.Core.Models;

namespace MovieApp.Core.Interfaces;

public interface IExternalReviewProvider
{
    Task<CriticReview?> GetReviewAsync(string movieTitle, int releaseYear, CancellationToken ct = default);
}


