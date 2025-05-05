using Avalonia.Controls;
using magazin_mercerie.ViewModels;
using magazin_mercerie.Views.ClientViews;
using Avalonia.Interactivity;

namespace magazin_mercerie.Views.LoginViews;
public partial class LoginClientView : UserControl
{
    private bool _isPasswordVisible = false;
    public LoginClientView()
    {
        InitializeComponent();
    }

    public void LoginClientCommand(object sender, RoutedEventArgs e)
    {
        // Get the parent LoginViewModel to switch views
        if (DataContext is LoginClientViewModel && this.Parent is ContentControl contentControl &&
            contentControl.Parent is Border border &&
            border.Parent is Window window &&
            window.DataContext is LoginViewModel loginViewModel)
        {
            // show the main window
            var mainWindow = new MainWindow();
            mainWindow.Show();
            window.Close();
        }
    }

    public void LoginAngajatCommand(object sender, RoutedEventArgs e)
    {
        // Get the parent LoginViewModel to switch views
        if (DataContext is LoginClientViewModel && this.Parent is ContentControl contentControl &&
            contentControl.Parent is Border border &&
            border.Parent is Window window &&
            window.DataContext is LoginViewModel loginViewModel)
        {
            // Create and set the AngajatView
            loginViewModel.CurrentView = new LoginAngajatView { DataContext = new LoginAngajatViewModel() };
        }
    }
    
    
    public void RegisterClientCommand(object sender, RoutedEventArgs e)
            {
                // Get the parent LoginViewModel to switch views
                if (DataContext is LoginClientViewModel && this.Parent is ContentControl contentControl &&
                    contentControl.Parent is Border border &&
                    border.Parent is Window window &&
                    window.DataContext is LoginViewModel loginViewModel)
                {
                    // Create and set the RegisterClientView
                    loginViewModel.CurrentView = new RegisterViews.RegisterClientView { DataContext = new RegisterClientViewModel() };
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