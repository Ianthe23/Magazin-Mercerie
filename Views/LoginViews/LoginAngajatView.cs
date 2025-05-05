using Avalonia.Controls;
using Avalonia.Interactivity;
using magazin_mercerie.ViewModels;
namespace magazin_mercerie.Views.LoginViews;

public partial class LoginAngajatView : UserControl
{
    private bool _isPasswordVisible = false;
    public LoginAngajatView()
    {
        InitializeComponent();
    }

    private void LoginClientCommand(object sender, RoutedEventArgs e)
    {
        // Get the parent LoginViewModel to switch views
        if (DataContext is LoginAngajatViewModel && this.Parent is ContentControl contentControl &&
            contentControl.Parent is Border border &&
            border.Parent is Window window &&
            window.DataContext is LoginViewModel loginViewModel)
        {
            // Create and set the ClientView
            loginViewModel.CurrentView = new LoginClientView { DataContext = new LoginClientViewModel() };
        }
    }

    private void RegisterAngajatCommand(object sender, RoutedEventArgs e)
    {
        // Get the parent LoginViewModel to switch views
        if (DataContext is LoginAngajatViewModel && this.Parent is ContentControl contentControl &&
            contentControl.Parent is Border border &&
            border.Parent is Window window &&
            window.DataContext is LoginViewModel loginViewModel)
        {
            // Create and set the ClientView
            loginViewModel.CurrentView = new RegisterViews.RegisterAngajatView { DataContext = new RegisterAngajatViewModel() };
        }
    }
    
    private void TogglePasswordVisibility(object sender, RoutedEventArgs e)
    {
        var passwordBox = this.FindControl<TextBox>("PasswordBox");
        if (passwordBox != null)
        {
            _isPasswordVisible = !_isPasswordVisible;
            passwordBox.PasswordChar = _isPasswordVisible ? '\0' : '*';
            
            // Update the icon
            var button = sender as Button;
            if (button != null)
            {
                var icon = button.Content as PathIcon;
                if (icon != null)
                {
                    icon.Data = _isPasswordVisible 
                        ? (Avalonia.Media.Geometry)this.FindResource("SemiIconEyeOpened") 
                        : (Avalonia.Media.Geometry)this.FindResource("SemiIconEyeClosedSolid");
                }
            }
        }
    }
    
}
