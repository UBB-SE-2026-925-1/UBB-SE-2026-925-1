// <copyright file="DetailsCheckoutPage.xaml.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.UI.Views;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using MovieApp.UI.Controls;
using MovieApp.UI.ViewModels.Events;

/// <summary>
/// Provides the event detail, seat-guide, and checkout layout that drives the ticket purchase flow.
/// </summary>
public sealed partial class DetailsCheckoutPage : Page
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DetailsCheckoutPage"/> class.
    /// </summary>
    public DetailsCheckoutPage()
    {
        this.InitializeComponent();
        this.DataContextChanged += (sender, args) => this.Bindings.Update();
    }

    /// <summary>
    /// Gets the current view model resolved from the page's data context.
    /// </summary>
    public DetailsCheckoutViewModel? ViewModel => this.DataContext as DetailsCheckoutViewModel;

    /// <summary>
    /// Initializes the page with the supplied movie and (optional) screening, wiring a fresh view model from DI.
    /// </summary>
    /// <param name="movie">The movie being checked out.</param>
    /// <param name="screening">An optional screening to pre-select.</param>
    /// <param name="onNavigateBack">Callback invoked when the user requests to leave the page.</param>
    public async Task InitializeAsync(Movie movie, Screening? screening, Action onNavigateBack)
    {
        if (movie is null)
        {
            throw new ArgumentNullException(nameof(movie));
        }

        IServiceProvider provider = App.ServiceProvider;
        DetailsCheckoutViewModel viewModel = new DetailsCheckoutViewModel(
            provider.GetRequiredService<IScreeningRepository>(),
            provider.GetRequiredService<IEventRepository>(),
            provider.GetRequiredService<IUserMovieDiscountRepository>(),
            provider.GetRequiredService<IPriceWatcherRepository>(),
            provider.GetRequiredService<IUserEventAttendanceRepository>(),
            provider.GetRequiredService<IBookingRepository>(),
            provider.GetService<IReferralValidator>(),
            App.CurrentUserId);

        viewModel.NavigateBack += () => onNavigateBack?.Invoke();
        this.DataContext = viewModel;
        this.Bindings.Update();

        await viewModel.LoadAsync(movie, screening);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        this.ViewModel?.RequestBack();
    }

    private async void OpenSeatGuide_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DetailsCheckoutViewModel? viewModel = this.ViewModel;
            if (viewModel?.CurrentEvent is null)
            {
                return;
            }

            SeatGuideViewModel seatGuide = await viewModel.GetOrCreateSeatGuideAsync();
            var snapshot = seatGuide.SnapshotSelection();

            SeatGuideDialog dialog = new SeatGuideDialog(seatGuide)
            {
                XamlRoot = this.XamlRoot,
            };

            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                viewModel.ApplyConfirmedSelection(seatGuide.SnapshotSelection());
            }
            else
            {
                seatGuide.RestoreSelection(snapshot);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OpenSeatGuide failed: {ex}");
        }
    }

    private async void ValidateReferral_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (this.ViewModel is { } viewModel)
            {
                await viewModel.ValidateReferralAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ValidateReferral failed: {ex}");
        }
    }

    private async void WatchPrice_Toggled(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not ToggleSwitch toggle || this.ViewModel is not { } viewModel)
            {
                return;
            }

            if (toggle.IsOn == viewModel.IsWatchingPrice)
            {
                return;
            }

            await viewModel.ToggleWatchPriceAsync();

            if (toggle.IsOn != viewModel.IsWatchingPrice)
            {
                toggle.IsOn = viewModel.IsWatchingPrice;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"WatchPrice toggle failed: {ex}");
        }
    }

    private async void ConfirmPurchase_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (this.ViewModel is { } viewModel)
            {
                await viewModel.ConfirmPurchaseAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ConfirmPurchase failed: {ex}");
        }
    }
}
