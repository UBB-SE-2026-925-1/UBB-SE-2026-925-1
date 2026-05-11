using System.Threading;
using System.Threading.Tasks;
#nullable enable

using MovieApp.Core.Interfaces;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Core.Models.DTOs;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MovieApp.Core.Services;

/// <summary>
/// Provides movie reviews sourced from the New York Times Article Search API.
/// </summary>
public sealed class NytReviewProvider : IExternalReviewProvider
{
    private const string ApiKey = "50k6GUkhjA7OiKdLuL11ucYiyffwBj4j640MCDVBdeQu9UXl";
    private readonly HttpClient httpClient;
    private readonly ICacheService cacheService;

    public NytReviewProvider(HttpClient httpClient, ICacheService cacheService)
    {
        this.httpClient = httpClient;
        this.cacheService = cacheService;
    }

    /// <inheritdoc/>
    public async Task<CriticReview?> GetReviewAsync(string movieTitle, int releaseYear, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(movieTitle)) return null;

        var context = await this.GetOmdbContextAsync(movieTitle, releaseYear, ct);
        var query = Uri.EscapeDataString($"\"{context.Title}\" \"{context.Year}\" {context.Director}");
        var fq = Uri.EscapeDataString("type_of_material:(\"Review\") AND subject:(\"Movies\")");

        var url = $"https://api.nytimes.com/svc/search/v2/articlesearch.json?q={query}&fq={fq}&sort=relevance&api-key={ApiKey}";
        var cacheKey = BuildCacheKey("nyt_full", movieTitle, releaseYear);

        var json = await this.cacheService.FetchOrCacheAsync(cacheKey, url, this.httpClient, ct);
        if (string.IsNullOrWhiteSpace(json)) return null;

        try
        {
            var dto = JsonSerializer.Deserialize<NytApiResponseDto>(json);
            var doc = dto?.Response?.Docs?
                .Where(d => IsSpecificMovieReview(context.Title, context.Year, d.Headline?.Main, d.Snippet))
                .OrderByDescending(d => MatchScore(movieTitle, releaseYear, d.Headline?.Main, d.Snippet))
                .FirstOrDefault();

            if (doc is null) return null;

            return new CriticReview
            {
                Source = "New York Times",
                Score = 0,
                Headline = doc.Headline?.Main ?? string.Empty,
                Snippet = BuildLongerSnippet(doc.Snippet, context.Title, context.Year),
                Url = doc.WebUrl
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private async Task<(string Title, int Year, string Director)> GetOmdbContextAsync(string movieTitle, int releaseYear, CancellationToken ct)
    {
        var omdbUrl = $"https://www.omdbapi.com/?apikey=57b3a80a&t={Uri.EscapeDataString(movieTitle)}&y={releaseYear}";
        var cacheKey = BuildCacheKey("nyt_omdb", movieTitle, releaseYear);

        var json = await this.cacheService.FetchOrCacheAsync(cacheKey, omdbUrl, this.httpClient, ct);
        if (string.IsNullOrWhiteSpace(json)) return (movieTitle, releaseYear, string.Empty);

        try
        {
            var dto = JsonSerializer.Deserialize<OmdbContextDto>(json);
            if (dto is null) return (movieTitle, releaseYear, string.Empty);

            var year = int.TryParse(dto.Year, out var y) ? y : releaseYear;
            var dir = dto.Director.Split(',')[0].Trim();

            return (dto.Title ?? movieTitle, year, dir);
        }
        catch (JsonException)
        {
            return (movieTitle, releaseYear, string.Empty);
        }

    }

    private static string BuildCacheKey(string provider, string movieTitle, int releaseYear)
    {
        var sanitized = new string(movieTitle.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch).ToArray());
        return $"{provider}_{sanitized}_{releaseYear}_v3".Replace(' ', '_').ToLowerInvariant();
    }

    private static int MatchScore(string movieTitle, int releaseYear, string? headline, string? snippet)
    {
        var text = $"{headline} {snippet}";
        var score = text.Contains(movieTitle, StringComparison.OrdinalIgnoreCase) ? 10 : 0;
        if (text.Contains(releaseYear.ToString())) score += 2;
        return score;
    }

    private static bool IsSpecificMovieReview(string movieTitle, int releaseYear, string? headline, string? snippet)
    {
        var combined = $"{headline} {snippet}";
        if (string.IsNullOrWhiteSpace(combined)) return false;

        return combined.Contains(movieTitle, StringComparison.OrdinalIgnoreCase) &&
               combined.Contains("review", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildLongerSnippet(string? snippet, string title, int year)
    {
        return $"{(snippet ?? "Matching review found.").Trim()} (Source: NYT review for '{title}' {year})";
    }

    private sealed class OmdbContextDto
    {
        [JsonPropertyName("Title")] public string Title { get; set; } = string.Empty;
        [JsonPropertyName("Year")] public string Year { get; set; } = string.Empty;
        [JsonPropertyName("Director")] public string Director { get; set; } = string.Empty;
    }
}

