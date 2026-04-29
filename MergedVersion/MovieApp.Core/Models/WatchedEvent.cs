// <copyright file="WatchedEvent.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>
namespace MovieApp.Core.Models;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Object to see properties of a watched event.
/// </summary>
public sealed class WatchedEvent
{
    /// <summary>
    /// Gets or sets the event Id.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int EventId { get; set; }

    /// <summary>
    /// Gets or sets the Event Title.
    /// </summary>
    public string EventTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target price.
    /// </summary>
    public decimal TargetPrice { get; set; }
}
