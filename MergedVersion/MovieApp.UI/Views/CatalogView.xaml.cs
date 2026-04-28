using Microsoft.UI.Xaml.Controls;
using MovieApp.Core.Models;
using MovieApp.UI.ViewModels;

namespace MovieApp.UI.Views;

public sealed partial class CatalogView : UserControl
{
    public CatalogViewModel? ViewModel => DataContext as CatalogViewModel;

    public CatalogView()
    {
        this.InitializeComponent();
        this.DataContextChanged += (s, e) => Bindings.Update();
        this.Loaded += (s, e) => ViewModel?.LoadMoviesCommand.Execute(null);
    }

    private void MovieList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Movie movie)
        {
            ViewModel?.SelectMovieCommand.Execute(movie);
        }
    }
}
