// <copyright file="SeatGuideViewModel.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.UI.ViewModels.Events;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MovieApp.Core.Models;

/// <summary>
/// Manages the layout, generation, and selection state of seating for a screening.
/// </summary>
public sealed class SeatGuideViewModel : ViewModelBase
{
    private const int DefaultColumnCount = 10;

    private int selectedCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeatGuideViewModel"/> class with no booked seats.
    /// </summary>
    /// <param name="totalCapacity">The total capacity of the venue.</param>
    public SeatGuideViewModel(int totalCapacity = Event.DefaultMaxCapacity)
        : this(totalCapacity, bookedSeats: (IReadOnlyCollection<(int, int)>?)null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SeatGuideViewModel"/> class
    /// with the actual booked seats marked unavailable.
    /// </summary>
    /// <param name="totalCapacity">The total capacity of the venue.</param>
    /// <param name="bookedSeats">A set of (row, column) tuples that are already booked.</param>
    public SeatGuideViewModel(int totalCapacity, IReadOnlyCollection<(int Row, int Column)>? bookedSeats)
    {
        if (totalCapacity <= 0)
        {
            totalCapacity = Event.DefaultMaxCapacity;
        }

        this.GenerateLayout(totalCapacity, bookedSeats ?? Array.Empty<(int, int)>());
    }

    /// <summary>
    /// Gets the collection of seats generated for the layout.
    /// </summary>
    public ObservableCollection<Seat> Seats { get; } = new ObservableCollection<Seat>();

    /// <summary>
    /// Gets the total number of rows in the layout.
    /// </summary>
    public int TotalRows { get; private set; }

    /// <summary>
    /// Gets the total number of columns in the layout.
    /// </summary>
    public int TotalColumns { get; private set; }

    /// <summary>
    /// Gets the count of currently selected seats.
    /// </summary>
    public int SelectedCount
    {
        get => this.selectedCount;
        private set => this.SetProperty(ref this.selectedCount, value);
    }

    /// <summary>
    /// Gets a snapshot of the seats the user has selected.
    /// </summary>
    public IReadOnlyList<Seat> SelectedSeats => this.Seats.Where(seat => seat.IsSelected).ToList();

    /// <summary>
    /// Toggles the selection state of an available seat.
    /// </summary>
    /// <param name="seat">The seat to toggle. Booked seats are ignored.</param>
    public void ToggleSeat(Seat? seat)
    {
        if (seat is null || !seat.IsAvailable)
        {
            return;
        }

        seat.IsSelected = !seat.IsSelected;
        this.SelectedCount = this.Seats.Count(currentSeat => currentSeat.IsSelected);
    }

    /// <summary>
    /// Clears the current seat selection.
    /// </summary>
    public void ClearSelection()
    {
        foreach (Seat seat in this.Seats)
        {
            if (seat.IsSelected)
            {
                seat.IsSelected = false;
            }
        }

        this.SelectedCount = 0;
    }

    private void GenerateLayout(int capacity, IReadOnlyCollection<(int Row, int Column)> bookedSeats)
    {
        this.Seats.Clear();
        this.SelectedCount = 0;

        this.TotalColumns = DefaultColumnCount;
        this.TotalRows = (int)Math.Ceiling((double)capacity / this.TotalColumns);

        int centerRow = (this.TotalRows / 2) + 1;
        int centerColumn = this.TotalColumns / 2;

        var booked = new HashSet<(int, int)>(bookedSeats);

        int currentSeatCount = 0;

        for (int row = 1; row <= this.TotalRows; row++)
        {
            for (int column = 1; column <= this.TotalColumns; column++)
            {
                if (currentSeatCount >= capacity)
                {
                    break;
                }

                Seat seat = new Seat { Row = row, Column = column };

                if (row <= 2 && this.TotalRows > 3)
                {
                    seat.Quality = SeatQuality.Poor;
                }
                else if (row == 1 && this.TotalRows <= 3)
                {
                    seat.Quality = SeatQuality.Poor;
                }
                else if (Math.Abs(row - centerRow) <= 1 && Math.Abs(column - centerColumn) <= 1)
                {
                    seat.Quality = SeatQuality.Optimal;
                    seat.IsSweetSpot = true;
                }
                else
                {
                    seat.Quality = SeatQuality.Standard;
                }

                if (booked.Contains((row, column)))
                {
                    seat.IsAvailable = false;
                }

                this.Seats.Add(seat);
                currentSeatCount++;
            }
        }
    }

    /// <summary>
    /// Captures the current selection state for later restoration.
    /// </summary>
    public IReadOnlyList<(int Row, int Column)> SnapshotSelection() =>
        this.Seats.Where(s => s.IsSelected).Select(s => (s.Row, s.Column)).ToList();

    /// <summary>
    /// Restores selection to a previously-captured snapshot.
    /// </summary>
    public void RestoreSelection(IReadOnlyList<(int Row, int Column)> snapshot)
    {
        var set = new HashSet<(int, int)>(snapshot ?? Array.Empty<(int, int)>());
        foreach (var seat in this.Seats)
        {
            seat.IsSelected = set.Contains((seat.Row, seat.Column));
        }

        this.SelectedCount = this.Seats.Count(s => s.IsSelected);
    }
}
