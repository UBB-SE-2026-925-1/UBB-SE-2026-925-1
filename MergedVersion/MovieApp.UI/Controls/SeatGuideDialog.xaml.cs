// <copyright file="SeatGuideDialog.xaml.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.UI.Controls;

using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MovieApp.Core.Models;
using MovieApp.UI.ViewModels.Events;

/// <summary>
/// Represents a dialog that displays a seat guide for an event and lets the user pick seats.
/// </summary>
public sealed partial class SeatGuideDialog : ContentDialog
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SeatGuideDialog"/> class with a freshly generated layout.
    /// </summary>
    /// <param name="totalCapacity">The total seating capacity used to generate the layout.</param>
    public SeatGuideDialog(int totalCapacity)
        : this(new SeatGuideViewModel(totalCapacity))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SeatGuideDialog"/> class using an existing view model
    /// so that prior selections are preserved across opens.
    /// </summary>
    /// <param name="viewModel">The seat guide view model to bind to.</param>
    public SeatGuideDialog(SeatGuideViewModel viewModel)
    {
        this.InitializeComponent();

        this.ViewModel = viewModel;
        this.DataContext = this.ViewModel;
    }

    /// <summary>
    /// Gets the view model associated with the dialog.
    /// </summary>
    public SeatGuideViewModel ViewModel { get; }

    /// <summary>
    /// Gets the seats that the user selected when the dialog was confirmed.
    /// </summary>
    public IReadOnlyList<Seat> SelectedSeats => this.ViewModel.SelectedSeats;

    /// <summary>
    /// Converts a boolean value to a <see cref="Visibility"/> value.
    /// </summary>
    /// <param name="value">The boolean value to convert.</param>
    /// <returns><see cref="Visibility.Visible"/> if <paramref name="value"/> is <c>true</c>; otherwise, <see cref="Visibility.Collapsed"/>.</returns>
    public static Visibility GetVisibility(bool value)
        => value ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>
    /// Converts a boolean value to the inverse <see cref="Visibility"/> value.
    /// </summary>
    /// <param name="value">The boolean value to convert.</param>
    /// <returns><see cref="Visibility.Collapsed"/> if <paramref name="value"/> is <c>true</c>; otherwise, <see cref="Visibility.Visible"/>.</returns>
    public static Visibility GetInverseVisibility(bool value)
        => value ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>
    /// Shows the sweet-spot icon only when the seat is a sweet spot AND not currently selected.
    /// Selected seats already display a selection check, so we hide the sweet-spot icon to avoid stacking glyphs.
    /// </summary>
    /// <param name="isSweetSpot">Whether the seat is a sweet spot.</param>
    /// <param name="isSelected">Whether the seat is currently selected.</param>
    /// <returns>The resolved visibility for the sweet-spot indicator.</returns>
    public static Visibility GetSweetSpotVisibility(bool isSweetSpot, bool isSelected)
        => isSweetSpot && !isSelected ? Visibility.Visible : Visibility.Collapsed;

    private void SeatGrid_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is Seat seat)
        {
            this.ViewModel.ToggleSeat(seat);
        }
    }
}
