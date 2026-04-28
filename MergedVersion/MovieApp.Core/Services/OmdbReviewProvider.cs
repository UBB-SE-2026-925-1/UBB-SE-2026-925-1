using System.Threading;
using System.Threading.Tasks;
#nullable enable

using MovieApp.Core.Interfaces;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Core.Models.DTOs;
using System.Globalization;
using System.Text.Json;

namespace MovieApp.Core.Services;

/// <summary>
/// Provides movie ratings and metadata sourced from the OMDb API.
/// </summary>
public sealed class OmdbReviewProvider : IExternalReviewProvider
{
    private readonly HttpClient httpClient;
    private readonly ICacheService cacheService;

    /// <summary>
    /// Initializes a new instance of the <see cref="OmdbReviewProvider"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client for network requests.</param>
    /// <param name="cacheService">The service used for caching responses.</param>
    public OmdbReviewProvider(HttpClient httpClient, ICacheService cacheService)
    {
        this.httpClient = httpClient;
        this.cacheService = cacheService;
    }

    /// <inheritdoc/>
    public async Task<CriticReview?> GetReviewAsync(string movieTitle, int releaseYear, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(movieTitle))
        {
            return null;
        }

        var url = $"https://www.omdbapi.com/?apikey=57b3a80a&t={Uri.EscapeDataString(movieTitle)}&y={releaseYear}";
        var cacheKey = BuildCacheKey("omdb", movieTitle, releaseYear);

        var json = await this.cacheService.FetchOrCacheAsync(cacheKey, url, this.httpClient, ct);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            var dto = JsonSerializer.Deserialize<OmdbResponseDto>(json);
            var firstRating = dto?.Ratings?.FirstOrDefault();

            if (firstRating is null)
            {
                return null;
            }

            return new CriticReview
            {
                Source = firstRating.Source,
                Score = ParseScore(firstRating.Value),
                Headline = $"{movieTitle} — OMDb rating",
                Snippet = BuildLongerSnippet(firstRating.Source, firstRating.Value, movieTitle, releaseYear),
                Url = $"https://www.omdbapi.com/?t={Uri.EscapeDataString(movieTitle)}"
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string BuildCacheKey(string provider, string movieTitle, int releaseYear)
    {
        var sanitized = new string(movieTitle.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch).ToArray());
        return $"{provider}_{sanitized}_{releaseYear}".Replace(' ', '_').ToLowerInvariant();
    }

    private static double ParseScore(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;
        var trimmed = value.Trim();

        if (trimmed.EndsWith('%') && double.TryParse(trimmed.TrimEnd('%'), NumberStyles.Number, CultureInfo.InvariantCulture, out var percent))
        {
            return Math.Round(Math.Clamp(percent / 20.0, 0, 5), 1);
        }

        if (trimmed.Contains('/'))
        {
            var parts = trimmed.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 &&
                double.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var num) &&
                double.TryParse(parts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var den) && den > 0)
            {
                return Math.Round(Math.Clamp((num / den) * 5.0, 0, 5), 1);
            }
        }

        return 0;
    }

    private static string BuildLongerSnippet(string source, string value, string movieTitle, int releaseYear)
    {
        return $"OMDb aggregated rating from {source}: {value}. This normalized score is used as an external critic signal for '{movieTitle}' ({releaseYear}).";
    }
}

