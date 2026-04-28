using Microsoft.UI.Xaml.Controls;
using MovieApp.UI.ViewModels;

namespace MovieApp.UI.Views;

public sealed partial class ProfileView : UserControl
{
    public ProfileViewModel? ViewModel => DataContext as ProfileViewModel;

    public ProfileView()
    {
        this.InitializeComponent();
        this.DataContextChanged += (s, e) => Bindings.Update();
    }
}
