using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.UI.ViewModels;

namespace MovieApp.UI.Views;

public sealed partial class ForumView : UserControl
{
    public ForumViewModel? ViewModel => DataContext as ForumViewModel;

    public ForumView()
    {
        this.InitializeComponent();
        this.DataContextChanged += (s, e) => Bindings.Update();
    }

    private void ReplyButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is HyperlinkButton btn && btn.Tag is int commentId)
        {
            ViewModel?.StartReplyCommand.Execute(commentId);
        }
    }
}
