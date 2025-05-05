namespace magazin_mercerie.ViewModels;

using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System.Threading.Tasks;
using System;
using magazin_mercerie.Views.ClientViews;
using Avalonia.Controls;
using Windows = Avalonia.Controls.Window;

public partial class LoginAngajatViewModel : ViewModelBase
{
    private readonly IService _service;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading = false;

    public string Username 
    { 
        get => _username; 
        set 
        { 
            _username = value; 
            OnPropertyChanged(); 
        } 
    }
    
    public string Password 
    { 
        get => _password; 
        set 
        { 
            _password = value; 
            OnPropertyChanged(); 
        } 
    }
    
    public string ErrorMessage 
    { 
        get => _errorMessage; 
        set 
        { 
            _errorMessage = value; 
            OnPropertyChanged(); 
        } 
    }
    
    public bool IsLoading 
    { 
        get => _isLoading; 
        set 
        { 
            _isLoading = value; 
            OnPropertyChanged(); 
        } 
    }
    
    public ICommand LoginCommand { get; }

    public LoginAngajatViewModel()
    {
        _service = App.ServiceProvider.GetService(typeof(IService)) as IService;
        LoginCommand = new AsyncRelayCommand(OnLoginAsync);
    }

    private async Task OnLoginAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Username and password cannot be empty";
                return;
            }
            
            var user = await _service.LoginAngajat(Username, Password);
            if (user != null)
            {
                // Login successful - open appropriate window based on user type
                Window mainWindow;
                mainWindow = new MainWindow();
                
                // if (user is Patron)
                // {
                //     employeeWindow = new PatronWindow(); // Create this window for patron
                // }
                // else
                // {
                //     employeeWindow = new AngajatWindow(); // Create this window for regular employee
                // }

                mainWindow.Show();
                
                // Close the login window
                var currentWindow = GetCurrentWindow();
                if (currentWindow != null)
                {
                    currentWindow.Close();
                }
            }
            else
            {
                ErrorMessage = "Invalid username or password";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private Windows GetCurrentWindow()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }
}
