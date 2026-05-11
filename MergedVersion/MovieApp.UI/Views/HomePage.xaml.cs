// <copyright file="HomePage.xaml.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.UI.Views;

using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;
using MovieApp.Core.Models;
using MovieApp.UI.Navigation;
using MovieApp.UI.Services;
using MovieApp.UI.ViewModels.Events;

/// <summary>
/// Represents the home page that displays events and provides navigation shortcuts.
/// </summary>
public sealed partial class HomePage : Page
{
    /// <summary>
    /// Builds dialog content for event details.
    /// </summary>
    private readonly EventDialogContentBuilder dialogBuilder;

    /// <summary>
    /// Indicates whether the page has already been initialized.
    /// </summary>
    private bool initialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomePage"/> class.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when required application services are not initialized.
    /// </exception>
    public HomePage()
    {
        if (App.Services.EventRepository == null)
        {
            throw new InvalidOperationException("EventRepository is not initialized.");
        }

        this.ViewModel = new HomeEventsViewModel(App.Services.EventRepository);
        this.dialogBuilder = new EventDialogContentBuilder(
            App.Services.ReferralValidator,
            App.Services.CurrentUserService);

        this.NavigationCacheMode = NavigationCacheMode.Required;
        this.InitializeComponent();
        this.DataContext = this.ViewModel;
        this.Loaded += this.HomePage_Loaded;
    }

    /// <summary>
    /// Gets the view model associated with this page.
    /// </summary>
    public HomeEventsViewModel ViewModel { get; }

    /// <summary>
    /// Called when the page is navigated to and initializes required data.
    /// </summary>
    /// <param name="e">The navigation event data.</param>
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        // Initialization moved to Loaded event since direct Content assignment bypasses this.
    }

    private async void HomePage_Loaded(object sender, RoutedEventArgs e)
    {
        App.EnsureServicesValid();

        if (!this.initialized)
        {
            this.initialized = true;

            try
            {
                if (this.IsAmbassadorSystemAvailable())
                {
                    if (App.Services.AmbassadorRepository is null)
                    {
                        throw new NullReferenceException("Ambassador repository is not initialized.");
                    }

                    User currentUser = App.Services.CurrentUserService!.CurrentUser;
                    string? existingCode =
                        await App.Services.AmbassadorRepository.GetReferralCodeAsync(currentUser.Id);
                    if (string.IsNullOrEmpty(existingCode))
                    {
                        MovieApp.Core.Services.ReferralCodeGenerator generator =
                            new MovieApp.Core.Services.ReferralCodeGenerator();
                        string newCode = generator.Generate(currentUser.Username, currentUser.Id);
                        await App.Services.AmbassadorRepository.CreateAmbassadorProfileAsync(
                            currentUser.Id,
                            newCode);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($">>> Ambassador setup error: {ex.Message}");
            }
        }

        try
        {
            await this.ViewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            if (ex.InnerException != null)
                System.Diagnostics.Debug.WriteLine($">>> Inner: {ex.InnerException.Message}");
        }
    }

    /// <summary>
    /// Determines whether the ambassador system is available.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the ambassador system is available; otherwise, <c>false</c>.
    /// </returns>
    private bool IsAmbassadorSystemAvailable()
    {
        return App.Services.AmbassadorRepository is not null
            && App.Services.CurrentUserService?.CurrentUser is not null;
    }

    private void ShortcutButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not HomeNavigationShortcut shortcut)
        {
            return;
        }

        if (App.CurrentMainWindow is not null)
        {
            App.CurrentMainWindow.NavigateToRoute(shortcut.RouteTag);
            return;
        }

        this.Frame.Navigate(AppRouteResolver.ResolvePageType(shortcut.RouteTag));
    }

    private void SearchBox_SearchTextChanged(object? sender, string searchText)
    {
        this.ViewModel.SetSearchText(searchText);
    }

    private void SortSelector_SortOptionChanged(object? sender, MovieApp.Core.EventLists.EventSortOption sortOption)
    {
        this.ViewModel.SetSortOption(sortOption);
    }

    private async void EventCardButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not Event selectedEvent)
        {
            return;
        }

        int? discountPercentage = null;

        if (App.Services.EventUserStateService is not null)
        {
            int percentage = await App.Services.EventUserStateService.GetDiscountForEventAsync(selectedEvent.Id);
            if (percentage > 0)
            {
                discountPercentage = percentage;
            }
        }

        ContentDialog dialog = await this.dialogBuilder.BuildDialogAsync(
            selectedEvent,
            this.XamlRoot,
            isJackpotEvent: false,
            discountPercentage: discountPercentage);

        await dialog.ShowAsync();
    }

    private async void JoinEventButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not Event selectedEvent)
        {
            return;
        }

        button.IsEnabled = false;

        if (App.Services.EventJoinService is not null)
        {
            string tag = button.Tag?.ToString() ?? string.Empty;
            JoinEventResult result = await App.Services.EventJoinService.JoinEventAsync(selectedEvent.Id, tag);
            button.Content = result.Message;
        }
    }
}
