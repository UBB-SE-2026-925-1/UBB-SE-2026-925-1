// <copyright file="RoomLayout.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Models;

/// <summary>
/// Deterministic room dimensions derived from a screening identifier so that
/// reloads of the same screening always show the same seat grid.
/// Rooms are between 10 and 15 rows, and each row has between 12 and 20 seats.
/// </summary>
public static class RoomLayout
{
    public const int MinRows = 10;
    public const int MaxRows = 15;
    public const int MinColumns = 12;
    public const int MaxColumns = 20;

    public static (int Rows, int Columns) For(int screeningId)
    {
        int seed = screeningId <= 0 ? 1 : screeningId;
        int rowSpan = MaxRows - MinRows + 1;
        int colSpan = MaxColumns - MinColumns + 1;
        int rows = MinRows + (seed * 7 % rowSpan);
        int columns = MinColumns + (seed * 13 % colSpan);
        return (rows, columns);
    }
}
