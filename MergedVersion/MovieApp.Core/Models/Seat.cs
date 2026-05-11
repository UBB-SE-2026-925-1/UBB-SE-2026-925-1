// <copyright file="Seat.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>
namespace MovieApp.Core.Models;

using System.ComponentModel;
using System.Runtime.CompilerServices;

/// <summary>
/// Defines the various quality levels assigned to venue seats.
/// </summary>
public enum SeatQuality
{
    /// <summary>
    /// Restricted view or less comfortable seating.
    /// </summary>
    Poor,

    /// <summary>
    /// Standard viewing experience.
    /// </summary>
    Standard,

    /// <summary>
    /// Prime viewing location with the best visibility.
    /// </summary>
    Optimal,
}

/// <summary>
/// Represents an individual seat within a theater or event venue.
/// </summary>
public sealed class Seat : INotifyPropertyChanged
{
    private const string ColorPoor = "#FF4D4D";
    private const string ColorOptimal = "#4CAF50";
    private const string ColorStandard = "#FFC107";
    private const string ColorDefault = "#E0E0E0";

    private bool isAvailable = true;
    private bool isSelected;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the row index of the seat.
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    /// Gets or sets the column index of the seat.
    /// </summary>
    public int Column { get; set; }

    /// <summary>
    /// Gets or sets the quality classification of the seat.
    /// </summary>
    public SeatQuality Quality { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the seat is in a "sweet spot" location.
    /// </summary>
    public bool IsSweetSpot { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the seat can be booked.
    /// </summary>
    public bool IsAvailable
    {
        get => this.isAvailable;
        set
        {
            if (this.isAvailable != value)
            {
                this.isAvailable = value;
                this.OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the seat is currently selected by the user.
    /// </summary>
    public bool IsSelected
    {
        get => this.isSelected;
        set
        {
            if (this.isSelected != value)
            {
                this.isSelected = value;
                this.OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets a label such as "B5" describing the seat position.
    /// </summary>
    public string Label => $"R{this.Row}C{this.Column}";

    /// <summary>
    /// Gets the hex color code associated with the seat's quality level.
    /// </summary>
    public string SeatColor => this.Quality switch
    {
        SeatQuality.Poor => ColorPoor,
        SeatQuality.Optimal => ColorOptimal,
        SeatQuality.Standard => ColorStandard,
        _ => ColorDefault
    };

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
