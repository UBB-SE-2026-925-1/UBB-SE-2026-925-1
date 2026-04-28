using System.Threading;
using System.Threading.Tasks;
#nullable enable

using MovieApp.Core.Interfaces;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;

namespace MovieApp.Core.Services;

/// <summary>
/// Orchestrates the retrieval and analysis of reviews from external providers.
/// </summary>
public sealed class ExternalReviewService
{
    private readonly IEnumerable<IExternalReviewProvider> providers;

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with" 
        /* ... remaining stop words ... */
    };

    public ExternalReviewService(IEnumerable<IExternalReviewProvider> providers)
    {
        this.providers = providers;
    }

    /// <summary>Retrieves critic reviews from all registered external sources.</summary>
    public async Task<List<CriticReview>> GetExternalReviewsAsync(string movieTitle, int releaseYear, CancellationToken ct = default)
    {
        var tasks = this.providers.Select(async provider =>
        {
            try
            {
                // Note: Providers would ideally also accept CancellationToken
                return await provider.GetReviewAsync(movieTitle, releaseYear);
            }
            catch
            {
                return null;
            }
        });

        var results = await Task.WhenAll(tasks);
        return results.Where(r => r != null).Cast<CriticReview>().ToList();
    }

    /// <summary>Analyzes common keywords within the provided reviews.</summary>
    public List<(string Word, int Count)> AnalyseLexicon(List<CriticReview> reviews)
    {
        var wordCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var review in reviews)
        {
            var content = $"{review.Snippet} {review.Headline}";
            var words = content.Split(new[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                var clean = word.ToLower().Trim();
                if (clean.Length < 3 || StopWords.Contains(clean)) continue;

                if (!wordCounts.TryAdd(clean, 1)) wordCounts[clean]++;
            }
        }

        return wordCounts.OrderByDescending(kv => kv.Value).Take(10).Select(kv => (kv.Key, kv.Value)).ToList();
    }
}

