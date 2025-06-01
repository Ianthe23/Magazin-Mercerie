namespace magazin_mercerie.ViewModels;

using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System.Threading.Tasks;
using System;
using magazin_mercerie.Views.ClientViews;
using Avalonia.Controls;
using Windows = Avalonia.Controls.Window;
using log4net;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using magazin_mercerie.Service;
using magazin_mercerie.Views.LoginViews;
using System.Linq;

public partial class LoginClientViewModel : ViewModelBase
{
    private readonly IService _service;
    private readonly IUserSessionService _userSessionService;
    private readonly ILog _logger;
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
    public ICommand LoginAngajatCommand { get; }

    public LoginClientViewModel()
    {
        try
        {
            _service = App.ServiceProvider?.GetService<IService>();
            _userSessionService = App.ServiceProvider?.GetService<IUserSessionService>();
            _logger = LogManager.GetLogger(typeof(LoginClientViewModel));
            _logger.Debug("LoginClientViewModel initialized");
            
            
            LoginCommand = new AsyncRelayCommand(LoginAsync);
            LoginAngajatCommand = new RelayCommand(OnLoginAngajat);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in LoginClientViewModel constructor: {ex.Message}");
            if (_logger != null)
            {
                _logger.Error("Error initializing LoginClientViewModel", ex);
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
            var user = await _service.LoginClient(Username, Password);
            if (user != null)
            {
                _logger?.Info($"Successful login for {Username}");
                Console.WriteLine($"Login successful for user: {Username}");
                
                // Set the current user in the session service
                _userSessionService?.SetCurrentUser(user);
                
                // ALSO register this user for the MainWindow specifically
                _userSessionService?.SetWindowUser("MainWindow", user);
                
                // Set success message
                SuccessMessage = $"Welcome {user.Nume}! Login successful.";
                
                try
                {
                    // Show success message for a brief moment (2-3 seconds)
                    await Task.Delay(2500); // 2.5 seconds delay
                    
                    // Login successful - open main window
                    _logger?.Debug("Creating MainWindow...");
                    var mainWindow = new MainWindow();
                    _logger?.Debug("MainWindow created successfully");
                    
                    // Show the window and set it as MainWindow before closing the login window
                    _logger?.Debug("Showing MainWindow...");
                    mainWindow.Show();
                    _logger?.Debug("MainWindow shown successfully");
                    
                    // KEEP LOGIN WINDOW OPEN: Don't set as MainWindow to allow multiple logins
                    // Don't change the MainWindow so login window stays as the main window
                    
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
                    _logger?.Error("Error creating or showing main window", ex);
                    Console.WriteLine($"ERROR creating/showing main window: {ex.Message}");
                    ErrorMessage = $"Error opening main window: {ex.Message}";
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
            
            // Show error popup for exceptions
            await ShowMessageBoxAsync("Login Error", $"An error occurred: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnLoginAngajat()
    {
        _logger.Debug("Switching to Employee login view");
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
    
    private async Task ShowMessageBoxAsync(string title, string message)
    {
        try
        {
            var currentWindow = GetCurrentWindow();
            if (currentWindow != null)
            {
                var messageBox = new Avalonia.Controls.Window
                {
                    Title = title,
                    Width = 350,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Content = new StackPanel
                    {
                        Margin = new Avalonia.Thickness(20),
                        Children =
                        {
                            new TextBlock 
                            { 
                                Text = message,
                                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                                Margin = new Avalonia.Thickness(0, 0, 0, 20)
                            },
                            new Button 
                            { 
                                Content = "OK",
                                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                                Width = 80
                            }
                        }
                    }
                };
                
                // Handle the button click to close the message box
                if (messageBox.Content is StackPanel panel && 
                    panel.Children.Count > 1 && 
                    panel.Children[1] is Button button)
                {
                    button.Click += (sender, e) => messageBox.Close();
                }
                
                // Show the dialog
                await messageBox.ShowDialog(currentWindow);
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Error showing message box: {ex.Message}", ex);
            Console.WriteLine($"Error showing message box: {ex.Message}");
        }
    }
}
