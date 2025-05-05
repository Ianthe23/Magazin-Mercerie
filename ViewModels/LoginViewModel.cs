namespace magazin_mercerie.ViewModels;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using magazin_mercerie.Views.LoginViews;
using System.Windows.Input;

public partial class LoginViewModel : ViewModelBase
{
    private UserControl? _currentView;

    public UserControl? CurrentView
    {
        get => _currentView;
        set
        {
            if (_currentView != value)
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }
    }
    public LoginViewModel()
    {
        CurrentView = new LoginClientView { DataContext = new LoginClientViewModel() };
    }
}
