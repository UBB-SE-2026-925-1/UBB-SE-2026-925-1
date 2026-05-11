// <copyright file="CatalogViewModel.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

using MovieApp.Core.Models;

namespace MovieApp.Web.Models;

/// <summary>
/// View model for the Catalog/Index page carrying movies, filter state, and pagination.
/// </summary>
public class CatalogViewModel
{
    /// <summary>Gets or sets the (paged) list of movies to display.</summary>
    public List<Movie> Movies { get; set; } = new();

    /// <summary>Gets or sets all available genres for the filter dropdown.</summary>
    public List<Genre> Genres { get; set; } = new();

    /// <summary>Gets or sets the active search query string.</summary>
    public string SearchQuery { get; set; } = string.Empty;

    /// <summary>Gets or sets the name of the genre filter currently selected, or empty for "All".</summary>
    public string SelectedGenre { get; set; } = string.Empty;

    /// <summary>Gets or sets the minimum average rating filter (0 = no filter).</summary>
    public float MinRating { get; set; }

    /// <summary>Gets or sets the current page number (1-based).</summary>
    public int Page { get; set; } = 1;

    /// <summary>Gets or sets the total number of pages.</summary>
    public int TotalPages { get; set; } = 1;

    /// <summary>Gets or sets the total number of matching movies (before pagination).</summary>
    public int TotalCount { get; set; }

    /// <summary>Gets a value indicating whether any filter is currently active.</summary>
    public bool HasActiveFilter =>
        !string.IsNullOrWhiteSpace(this.SearchQuery) ||
        !string.IsNullOrWhiteSpace(this.SelectedGenre) ||
        this.MinRating > 0;
}
