using Microsoft.UI.Xaml.Controls;
using MovieApp.UI.ViewModels;

namespace MovieApp.UI.Views;

public sealed partial class BattleView : UserControl
{
    public BattleViewModel? ViewModel => DataContext as BattleViewModel;

    public BattleView()
    {
        this.InitializeComponent();
        this.DataContextChanged += (s, e) => Bindings.Update();
    }

    private void BetMovieSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel != null && e.AddedItems.Count > 0 && e.AddedItems[0] is MovieApp.Core.Models.Movie movie)
        {
            ViewModel.SelectedBetMovieId = movie.Id;
        }
    }
}
