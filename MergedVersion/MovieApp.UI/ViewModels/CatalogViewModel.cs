#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using CommunityToolkit.Mvvm.Input;

namespace MovieApp.UI.ViewModels;

/// <summary>
/// ViewModel for the movie catalog view with search and filter capabilities.
/// </summary>
public class CatalogViewModel : ViewModelBase
{
    private readonly ICatalogService catalogService;
    private string searchQuery = string.Empty;
    private string selectedGenre = "All Genres";
    private double minimumRating;
    private Movie? selectedMovie;

    /// <summary>
    /// Event raised when a movie is selected for detail view.
    /// </summary>
    public event Action<Movie>? MovieSelected;

    /// <summary>
    /// Initializes a new instance of <see cref="CatalogViewModel"/>.
    /// </summary>
    /// <param name="catalogService">The catalog service.</param>
    public CatalogViewModel(ICatalogService catalogService)
    {
        this.catalogService = catalogService;

        // Commands
        this.SelectMovieCommand = new RelayCommand<object>(param =>
        {
            if (param is Movie movie)
            {
                this.SelectedMovie = movie;
                this.MovieSelected?.Invoke(movie);
            }
        });

        this.LoadMoviesCommand = new AsyncRelayCommand(this.LoadMoviesAsync);
        this.ClearFiltersCommand = new AsyncRelayCommand(this.ClearFiltersAsync);
    }

    /// <summary>Gets the collection of movies to display.</summary>
    public ObservableCollection<Movie> Movies { get; } = new ();

    /// <summary>Gets the list of available genres.</summary>
    public ObservableCollection<string> Genres { get; } = new ()
    {
        "All Genres", "Action", "Adventure", "Animation", "Comedy", "Drama", "Horror", "Mystery", "Romance", "Science Fiction", "Thriller"
    };

    /// <summary>Gets or sets the search query text.</summary>
    public string SearchQuery
    {
        get => this.searchQuery;
        set
        {
            if (this.SetProperty(ref this.searchQuery, value))
            {
                _ = this.UpdateResultsAsync();
            }
        }
    }

    /// <summary>Gets or sets the selected genre filter.</summary>
    public string SelectedGenre
    {
        get => this.selectedGenre;
        set
        {
            if (this.SetProperty(ref this.selectedGenre, value))
            {
                _ = this.UpdateResultsAsync();
            }
        }
    }

    /// <summary>Gets or sets the minimum rating filter.</summary>
    public double MinimumRating
    {
        get => this.minimumRating;
        set
        {
            if (this.SetProperty(ref this.minimumRating, value))
            {
                _ = this.UpdateResultsAsync();
            }
        }
    }

    /// <summary>Gets or sets the currently selected movie.</summary>
    public Movie? SelectedMovie
    {
        get => this.selectedMovie;
        set => this.SetProperty(ref this.selectedMovie, value);
    }

    /// <summary>Gets the command to select a movie.</summary>
    public ICommand SelectMovieCommand { get; }

    /// <summary>Gets the command to load all movies initially.</summary>
    public ICommand LoadMoviesCommand { get; }

    /// <summary>Gets the command to clear all active filters and search.</summary>
    public ICommand ClearFiltersCommand { get; }

    /// <summary>
    /// Loads all movies from the catalog.
    /// </summary>
    public async Task LoadMoviesAsync()
    {
        System.Diagnostics.Debug.WriteLine(">>> CatalogViewModel: Loading movies...");
        var movies = await this.catalogService.GetAllMoviesAsync();
        System.Diagnostics.Debug.WriteLine($">>> CatalogViewModel: Found {movies.Count} movies.");
        
        this.Movies.Clear();
        foreach (var movie in movies)
        {
            this.Movies.Add(movie);
        }
    }

    /// <summary>
    /// Unified method that applies Search, Genre, and Rating simultaneously.
    /// </summary>
    private async Task UpdateResultsAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($">>> CatalogViewModel: Updating results (Search: '{this.SearchQuery}', Genre: '{this.SelectedGenre}')");
            IEnumerable<Movie> currentMovies;

            if (string.IsNullOrWhiteSpace(this.SearchQuery))
            {
                currentMovies = await this.catalogService.GetAllMoviesAsync();
            }
            else
            {
                currentMovies = await this.catalogService.SearchMoviesAsync(this.SearchQuery);
            }

            // Apply Genre filter
            if (!string.IsNullOrWhiteSpace(this.SelectedGenre) && this.SelectedGenre != "All Genres")
            {
                currentMovies = currentMovies.Where(m => m.Genres != null && m.Genres.Any(g => g.Name == this.SelectedGenre));
            }

            // Apply Rating filter
            if (this.MinimumRating > 0)
            {
                currentMovies = currentMovies.Where(m => m.AverageRating >= this.MinimumRating);
            }

            var resultsList = currentMovies.ToList();
            System.Diagnostics.Debug.WriteLine($">>> CatalogViewModel: Filtered to {resultsList.Count} results.");

            this.Movies.Clear();
            foreach (var movie in resultsList)
            {
                this.Movies.Add(movie);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($">>> SEARCH CRASHED: {ex.Message}");
            System.Diagnostics.Debug.WriteLine(ex.StackTrace);
        }
    }

    /// <summary>
    /// Resets filters.
    /// </summary>
    private async Task ClearFiltersAsync()
    {
        bool needsUpdate = false;

        if (!string.IsNullOrWhiteSpace(this.searchQuery))
        {
            this.SetProperty(ref this.searchQuery, string.Empty, nameof(this.SearchQuery));
            needsUpdate = true;
        }

        if (this.selectedGenre != "All Genres")
        {
            this.SetProperty(ref this.selectedGenre, "All Genres", nameof(this.SelectedGenre));
            needsUpdate = true;
        }

        if (this.minimumRating > 0)
        {
            this.SetProperty(ref this.minimumRating, 0, nameof(this.MinimumRating));
            needsUpdate = true;
        }

        if (needsUpdate)
        {
            await this.UpdateResultsAsync();
        }
    }

}
