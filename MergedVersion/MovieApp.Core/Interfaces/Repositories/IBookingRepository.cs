// <copyright file="IBookingRepository.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Repositories;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;

/// <summary>
/// Provides persistence operations for seat bookings tied to screenings.
/// </summary>
public interface IBookingRepository
{
    /// <summary>
    /// Returns all bookings for the given screening.
    /// </summary>
    Task<IReadOnlyList<SeatBooking>> GetByScreeningAsync(int screeningId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reserves the supplied seats for the screening atomically.
    /// Returns true on success; false if any of the seats are already booked.
    /// </summary>
    Task<bool> ReserveAsync(int screeningId, int userId, IReadOnlyList<(int Row, int Column)> seats, CancellationToken cancellationToken = default);
}
