namespace magazin_mercerie.ViewModels;

using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using magazin_mercerie.Views.ClientViews;
using Avalonia.Controls;
using Windows = Avalonia.Controls.Window;
using log4net;

public partial class LoginAngajatViewModel : ViewModelBase
{
    private readonly ILog _logger;
    private readonly IService _service;
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
            _service = App.ServiceProvider.GetService(typeof(IService)) as IService;
            _logger = LogManager.GetLogger(typeof(LoginAngajatViewModel));
            LoginCommand = new AsyncRelayCommand(OnLoginAsync);
            LoginClientCommand = new RelayCommand(OnLoginClient);

            _logger.Debug("LoginAngajatViewModel initialized");
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

    private async Task OnLoginAsync()
    {
        try
        {
            _logger.Debug($"Login attempt started with username: {Username}");
            Console.WriteLine($"Login attempt started with username: {Username}");
            
            // Validate inputs
            bool hasValidationErrors = false;
            
            if (string.IsNullOrWhiteSpace(Username))
            {
                UsernameHasError = true;
                hasValidationErrors = true;
            }
            
            if (string.IsNullOrWhiteSpace(Password))
            {
                PasswordHasError = true;
                hasValidationErrors = true;
            }
            
            if (hasValidationErrors)
            {
                ErrorMessage = "Username and password cannot be empty";
                _logger.Warn("Login attempt with empty username or password");
                return;
            }
            
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            
            _logger.Debug("Calling service.LoginAngajat...");
            var user = await _service.LoginAngajat(Username, Password);
            
            if (user != null)
            {
                _logger.Info($"Successful login for {Username}");
                Console.WriteLine($"Login successful for user: {Username}");
                
                // Set success message
                SuccessMessage = $"Welcome {user.Nume}! Login successful.";
                
                try
                {
                    // Show success message for a brief moment (2-3 seconds)
                    await Task.Delay(2500); // 2.5 seconds delay
                    
                    // Login successful - open main window
                    _logger.Debug("Creating MainWindow...");
                    var mainWindow = new MainWindow();
                    _logger.Debug("MainWindow created successfully");
                    
                    // Show the window before closing the login window
                    _logger.Debug("Showing employee window...");
                    mainWindow.Show();
                    _logger.Debug("Employee window shown successfully");
                    
                    // Close the login window
                    var currentWindow = GetCurrentWindow();
                    if (currentWindow != null)
                    {
                        _logger.Debug("Closing login window...");
                        currentWindow.Close();
                        _logger.Debug("Login window closed");
                    }
                    else
                    {
                        _logger.Error("Could not find current window to close");
                        Console.WriteLine("ERROR: Could not find current window to close");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Error creating or showing main window", ex);
                    Console.WriteLine($"ERROR creating/showing main window: {ex.Message}");
                    ErrorMessage = $"Error opening main window: {ex.Message}";
                }
            }
            else
            {
                ErrorMessage = "Invalid username or password";
                _logger.Warn($"Failed login for username: {Username}");
                Console.WriteLine($"Login failed for user: {Username}");
                
                // Show error popup for login failures
                // await ShowMessageBoxAsync("Login Failed", "Invalid username or password. Please try again.");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login error: {ex.Message}";
            _logger.Error($"Login error: {ex.Message}", ex);
            Console.WriteLine($"ERROR during login: {ex.Message}");
            
            // Show error popup for exceptions
            // await ShowMessageBoxAsync("Login Error", $"An error occurred: {ex.Message}");
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
