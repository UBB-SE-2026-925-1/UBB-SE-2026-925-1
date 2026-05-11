// <copyright file="CatalogController.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Proxy;
using MovieApp.Web.Models;

namespace MovieApp.Web.Controllers;

/// <summary>
/// Serves the movie catalog pages: listing, search, and genre/rating filtering.
/// </summary>
public class CatalogController : Controller
{
    private const int PageSize = 12;
    private const string GenresCacheKey = "catalog_genres";

    private readonly ICatalogService catalogService;
    private readonly ApiClient apiClient;
    private readonly IMemoryCache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="CatalogController"/> class.
    /// </summary>
    public CatalogController(ICatalogService catalogService, ApiClient apiClient, IMemoryCache cache)
    {
        this.catalogService = catalogService;
        this.apiClient = apiClient;
        this.cache = cache;
    }

    /// <summary>
    /// Displays the movie catalog with optional search, genre, and rating filters.
    /// </summary>
    /// <param name="q">Free-text title search query.</param>
    /// <param name="genre">Genre name to filter by (empty = all genres).</param>
    /// <param name="minRating">Minimum average rating (0 = no filter).</param>
    /// <param name="page">1-based page number for pagination.</param>
    [HttpGet]
    public async Task<IActionResult> Index(
        string? q,
        string? genre,
        float minRating = 0,
        int page = 1)
    {
        // ── Genres (memory-cached for 1 hour because they almost never change) ──
        List<Genre> genres;
        try
        {
            genres = await this.cache.GetOrCreateAsync(GenresCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                var result = await this.apiClient.GetAsync<List<Genre>>("api/movies/genres");
                return result ?? new List<Genre>();
            }) ?? new List<Genre>();
        }
        catch
        {
            genres = new List<Genre>();
        }

        // ── Movies ──────────────────────────────────────────────────────────────
        List<Movie> movies;
        try
        {
            if (!string.IsNullOrWhiteSpace(q))
            {
                movies = await this.catalogService.SearchMoviesAsync(q);
            }
            else if (!string.IsNullOrWhiteSpace(genre) || minRating > 0)
            {
                var genreList = string.IsNullOrWhiteSpace(genre)
                    ? new List<Genre>()
                    : new List<Genre> { new Genre { Name = genre } };

                movies = await this.catalogService.FilterMoviesAsync(genreList, minRating);
            }
            else
            {
                movies = await this.catalogService.GetAllMoviesAsync();
            }
        }
        catch (Exception ex)
        {
            movies = new List<Movie>();
            ViewBag.ErrorMessage = $"Could not load movies — the API may be unavailable. ({ex.Message})";
        }

        // ── Pagination ───────────────────────────────────────────────────────────
        if (page < 1)
        {
            page = 1;
        }

        var totalCount = movies.Count;
        var totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)PageSize);

        if (page > totalPages)
        {
            page = totalPages;
        }

        var pagedMovies = movies
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        // ── View model ──────────────────────────────────────────────────────────
        var vm = new CatalogViewModel
        {
            Movies = pagedMovies,
            Genres = genres,
            SearchQuery = q ?? string.Empty,
            SelectedGenre = genre ?? string.Empty,
            MinRating = minRating,
            Page = page,
            TotalPages = totalPages,
            TotalCount = totalCount,
        };

        return View(vm);
    }
}
