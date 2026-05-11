namespace MovieApp.UI;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.UI.Views;
using MovieApp.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using MovieApp.Core.Models;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        
        // Default to Catalog
        AppNavigationView.SelectedItem = AppNavigationView.MenuItems[0];
    }

    public void NavigateToRoute(string routeTag)
    {
        this.NavigateToTag(routeTag);
    }

    private void AppNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            string tag = item.Tag.ToString() ?? "";
            NavigateToTag(tag);
        }
    }

    private void NavigateToTag(string tag)
    {
        // Hide overlay when navigating
        MovieDetailOverlay.Visibility = Visibility.Collapsed;

        FrameworkElement? view = null;
        
        switch (tag)
        {
            case "Catalog":
                var catalogView = new CatalogView();
                catalogView.DataContext = App.ServiceProvider.GetRequiredService<CatalogViewModel>();
                // Wire up movie selection to show overlay
                if (catalogView.DataContext is CatalogViewModel vm)
                {
                    vm.MovieSelected += movie => ShowMovieDetail(movie);
                }
                view = catalogView;
                break;
                
            case "Battle":
                var battleView = new BattleView();
                battleView.DataContext = App.ServiceProvider.GetRequiredService<BattleViewModel>();
                _ = ((BattleViewModel)battleView.DataContext).LoadBattleAsync();
                view = battleView;
                break;
                
            case "Forum":
                var forumView = new ForumView();
                forumView.DataContext = App.ServiceProvider.GetRequiredService<ForumViewModel>();
                _ = ((ForumViewModel)forumView.DataContext).LoadMoviesAsync();
                view = forumView;
                break;
                
            case "Profile":
                var profileView = new ProfileView();
                profileView.DataContext = App.ServiceProvider.GetRequiredService<ProfileViewModel>();
                _ = ((ProfileViewModel)profileView.DataContext).LoadProfileAsync();
                view = profileView;
                break;

            case "Home":
                var homeView = new HomePage();
                view = homeView;
                break;

            case "Favorites":
                var favView = new FavoritesPage();
                favView.DataContext = App.ServiceProvider.GetRequiredService<FavoritesViewModel>();
                view = favView;
                break;

            case "MyEvents":
                var myEventsView = new MyEventsPage();
                // Assuming MyEventsPage uses its own logic or shared VM
                view = myEventsView;
                break;

            case "EventManagement":
                var eventManagementView = new EventManagementPage();
                view = eventManagementView;
                break;

            case "SlotMachine":
                var slotView = new SlotMachinePage();
                slotView.DataContext = App.ServiceProvider.GetRequiredService<SlotMachineViewModel>();
                view = slotView;
                break;

            case "TriviaWheel":
                var triviaView = new TriviaWheelPage();
                triviaView.DataContext = App.ServiceProvider.GetRequiredService<TriviaWheelViewModel>();
                view = triviaView;
                break;

            case "Marathons":
                var marathonView = new MarathonsPage();
                marathonView.DataContext = App.ServiceProvider.GetRequiredService<MarathonPageViewModel>();
                view = marathonView;
                break;

            case "Notifications":
                var notifyView = new NotificationsPage();
                notifyView.DataContext = App.ServiceProvider.GetRequiredService<NotificationsViewModel>();
                view = notifyView;
                break;

            case "Rewards":
                var rewardsView = new RewardsPage();
                rewardsView.DataContext = App.ServiceProvider.GetRequiredService<RewardsViewModel>();
                view = rewardsView;
                break;

            case "ReferralArea":
                var referralView = new ReferralAreaPage();
                view = referralView;
                break;

            // Team A Placeholders
            default:
                // For now, show a placeholder for unimplemented Team A views
                ContentFrame.Content = new TextBlock 
                { 
                    Text = $"View for {tag} not yet ported.", 
                    HorizontalAlignment = HorizontalAlignment.Center, 
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 24
                };
                return;
        }

        if (view != null)
        {
            ContentFrame.Content = view;
        }
    }

    private async void ShowMovieDetail(Movie movie)
    {
        var vm = App.ServiceProvider.GetRequiredService<MovieDetailViewModel>();
        MovieDetailViewControl.DataContext = vm;

        // Handle back navigation
        vm.NavigateBack += () => MovieDetailOverlay.Visibility = Visibility.Collapsed;

        await vm.LoadMovieAsync(movie);
        MovieDetailOverlay.Visibility = Visibility.Visible;
    }

    public async void ShowDetailsCheckout(Movie movie, Screening screening)
    {
        try
        {
            MovieDetailOverlay.Visibility = Visibility.Collapsed;

            var page = new DetailsCheckoutPage();
            ContentFrame.Content = page;

            await page.InitializeAsync(movie, screening, () =>
            {
                // Return to catalog when the user backs out of the checkout page.
                AppNavigationView.SelectedItem = AppNavigationView.MenuItems[0];
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ShowDetailsCheckout failed: {ex}");
        }
    }
}
