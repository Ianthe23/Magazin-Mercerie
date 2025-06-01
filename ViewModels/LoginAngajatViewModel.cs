namespace magazin_mercerie.ViewModels;

using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using magazin_mercerie.Views.ClientViews;
using magazin_mercerie.Views.PatronViews;
using magazin_mercerie.Views.AngajatViews;
using Avalonia.Controls;
using Windows = Avalonia.Controls.Window;
using log4net;
using magazin_mercerie.Service;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

public partial class LoginAngajatViewModel : ViewModelBase
{
    private readonly ILog _logger;
    private readonly IService _service;
    private readonly IUserSessionService _userSessionService;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isLoading = false;
    private bool _usernameHasError = false;
    private bool _passwordHasError = false;

    public string Username 
    { 
        get => _username; 
        set 
        { 
            _username = value; 
            UsernameHasError = string.IsNullOrWhiteSpace(value);
            OnPropertyChanged(); 
        } 
    }
    
    public string Password 
    { 
        get => _password; 
        set 
        { 
            _password = value; 
            PasswordHasError = string.IsNullOrWhiteSpace(value);
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
    
    public string SuccessMessage 
    { 
        get => _successMessage; 
        set 
        { 
            _successMessage = value; 
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
    
    public bool UsernameHasError
    {
        get => _usernameHasError;
        set
        {
            _usernameHasError = value;
            OnPropertyChanged();
        }
    }
    
    public bool PasswordHasError
    {
        get => _passwordHasError;
        set
        {
            _passwordHasError = value;
            OnPropertyChanged();
        }
    }
    
    public ICommand LoginCommand { get; }
    public ICommand LoginClientCommand { get; }

    public LoginAngajatViewModel()
    {
        try
        {
            _service = App.ServiceProvider?.GetService<IService>();
            _userSessionService = App.ServiceProvider?.GetService<IUserSessionService>();
            _logger = LogManager.GetLogger(typeof(LoginAngajatViewModel));
            LoginCommand = new AsyncRelayCommand(LoginAsync);
            LoginClientCommand = new RelayCommand(OnLoginClient);

            _logger?.Info("LoginAngajatViewModel initialized");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in LoginAngajatViewModel constructor: {ex.Message}");
            if (_logger != null)
            {
                _logger.Error("Error initializing LoginAngajatViewModel", ex);
            }
        }
    }

    private async Task LoginAsync()
    {
        _logger?.Debug($"Login attempt for username: {Username}");
        Console.WriteLine($"Login attempt for user: {Username}");
        
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter both username and password";
            _logger?.Warn("Login attempted with empty username or password");
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var user = await _service.LoginAngajat(Username, Password);
            if (user != null)
            {
                _logger?.Info($"Successful login for {Username}");
                Console.WriteLine($"Login successful for user: {Username}");
                
                // Set the current user in the session service
                _userSessionService?.SetCurrentUser(user);
                
                // Set success message
                SuccessMessage = $"Welcome {user.Nume}! Login successful.";
                
                try
                {
                    // Show success message for a brief moment (2-3 seconds)
                    await Task.Delay(2500); // 2.5 seconds delay
                    
                    // Check user type and route to appropriate window
                    if (user is Patron)
                    {
                        // Patron - route to PatronWindow
                        _logger?.Debug("User is Patron - Creating PatronWindow...");
                        
                        // Register user for the PatronWindow specifically
                        _userSessionService?.SetWindowUser("PatronWindow", user);
                        
                        var patronWindow = new PatronWindow();
                        _logger?.Debug("PatronWindow created successfully");
                        
                        _logger?.Debug("Showing patron window...");
                        patronWindow.Show();
                        _logger?.Debug("Patron window shown successfully");
                    }
                    else
                    {
                        // Regular Employee - route to AngajatWindow
                        _logger?.Debug("User is Employee - Creating AngajatWindow...");
                        
                        // Register user for a unique AngajatWindow identifier using employee ID
                        _userSessionService?.SetEmployeeForAngajatWindow(user.Id, (Angajat)user);
                        _logger?.Debug($"Registered employee {user.Username} (ID: {user.Id}) for unique AngajatWindow");
                        
                        var angajatWindow = new AngajatWindow(user.Id);
                        _logger?.Debug("AngajatWindow created successfully");
                        
                        _logger?.Debug("Showing employee window...");
                        angajatWindow.Show();
                        _logger?.Debug("Employee window shown successfully");
                    }
                    
                    // KEEP LOGIN WINDOW OPEN: Don't close the login window
                    // This allows multiple users to login from the same application instance
                    _logger?.Debug("Keeping login window open for additional logins...");
                    
                    // Clear the form for next user
                    Username = string.Empty;
                    Password = string.Empty;
                    SuccessMessage = string.Empty;
                }
                catch (Exception ex)
                {
                    _logger?.Error("Error creating or showing window", ex);
                    Console.WriteLine($"ERROR creating/showing window: {ex.Message}");
                    ErrorMessage = $"Error opening window: {ex.Message}";
                }
            }
            else
            {
                ErrorMessage = "Invalid username or password";
                _logger?.Warn($"Failed login for username: {Username}");
                Console.WriteLine($"Login failed for user: {Username}");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login error: {ex.Message}";
            _logger?.Error($"Login error: {ex.Message}", ex);
            Console.WriteLine($"ERROR during login: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private void OnLoginClient()
    {
        _logger.Debug("Switching to Client login view");
    }
    
    private Windows GetCurrentWindow()
    {
        try
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.MainWindow;
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error("Error getting current window", ex);
            Console.WriteLine($"ERROR getting current window: {ex.Message}");
            return null;
        }
    }
    
    // private async Task ShowMessageBoxAsync(string title, string message)
    // {
    //     try
    //     {
    //         var currentWindow = GetCurrentWindow();
    //         if (currentWindow != null)
    //         {
    //             var messageBox = new Avalonia.Controls.Window
    //             {
    //                 Title = title,
    //                 Width = 350,
    //                 Height = 200,
    //                 WindowStartupLocation = WindowStartupLocation.CenterOwner,
    //                 Content = new StackPanel
    //                 {
    //                     Margin = new Avalonia.Thickness(20),
    //                     Children =
    //                     {
    //                         new TextBlock 
    //                         { 
    //                             Text = message,
    //                             TextWrapping = Avalonia.Media.TextWrapping.Wrap,
    //                             Margin = new Avalonia.Thickness(0, 0, 0, 20)
    //                         },
    //                         new Button 
    //                         { 
    //                             Content = "OK",
    //                             HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
    //                             Width = 80
    //                         }
    //                     }
    //                 }
    //             };
                
    //             // Handle the button click to close the message box
    //             if (messageBox.Content is StackPanel panel && 
    //                 panel.Children.Count > 1 && 
    //                 panel.Children[1] is Button button)
    //             {
    //                 button.Click += (sender, e) => messageBox.Close();
    //             }
                
    //             // Show the dialog
    //             await messageBox.ShowDialog(currentWindow);
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.Error($"Error showing message box: {ex.Message}", ex);
    //         Console.WriteLine($"Error showing message box: {ex.Message}");
    //     }
    // }
}
