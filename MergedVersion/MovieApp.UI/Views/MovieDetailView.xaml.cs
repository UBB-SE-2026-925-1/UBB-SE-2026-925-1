using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.Core.Models;
using MovieApp.UI.ViewModels;

namespace MovieApp.UI.Views;

public sealed partial class MovieDetailView : UserControl
{
    public MovieDetailViewModel? ViewModel => DataContext as MovieDetailViewModel;

    public MovieDetailView()
    {
        this.InitializeComponent();
        this.DataContextChanged += (s, e) => Bindings.Update();
    }

    private void ReplyButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int commentId)
        {
            ViewModel?.StartReplyCommand.Execute(commentId);
        }
    }

    private void ScreeningTicketsButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button
            && button.Tag is Screening screening
            && this.ViewModel?.Movie is Movie movie)
        {
            App.CurrentMainWindow.ShowDetailsCheckout(movie, screening);
        }
    }
}
