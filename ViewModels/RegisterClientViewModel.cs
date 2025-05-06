namespace magazin_mercerie.ViewModels;

using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using magazin_mercerie.Views.LoginViews;
using Avalonia.Controls;
using Windows = Avalonia.Controls.Window;
using log4net;
using System.Linq;

public partial class RegisterClientViewModel : ViewModelBase
{   
    private readonly IService _service;
    private readonly ILog _logger;
    private string _name = string.Empty;
    private string _email = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _phone = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isLoading = false;
    private bool _nameHasError = false;
    private bool _emailHasError = false;
    private bool _usernameHasError = false;
    private bool _passwordHasError = false;
    private bool _phoneHasError = false;

    public string Name 
    { 
        get => _name; 
        set 
        { 
            _name = value; 
            NameHasError = string.IsNullOrWhiteSpace(value);
            OnPropertyChanged(); 
        } 
    }
    
    public string Email 
    { 
        get => _email; 
        set 
        { 
            _email = value; 
            EmailHasError = string.IsNullOrWhiteSpace(value) || !IsValidEmail(value);
            OnPropertyChanged(); 
        } 
    }
    
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
            PasswordHasError = string.IsNullOrWhiteSpace(value) || value.Length < 4;
            OnPropertyChanged(); 
        } 
    }
    
    public string Phone 
    { 
        get => _phone; 
        set 
        { 
            _phone = value; 
            PhoneHasError = string.IsNullOrWhiteSpace(value) || !IsValidPhoneNumber(value);
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
    
    public bool NameHasError
    {
        get => _nameHasError;
        set
        {
            _nameHasError = value;
            OnPropertyChanged();
        }
    }
    
    public bool EmailHasError
    {
        get => _emailHasError;
        set
        {
            _emailHasError = value;
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
    
    public bool PhoneHasError
    {
        get => _phoneHasError;
        set
        {
            _phoneHasError = value;
            OnPropertyChanged();
        }
    }
    
    public ICommand RegisterCommand { get; }

    public RegisterClientViewModel()
    {
        _service = App.ServiceProvider.GetService(typeof(IService)) as IService;
        _logger = LogManager.GetLogger(typeof(RegisterClientViewModel));
        _logger.Debug("RegisterClientViewModel initialized");
        RegisterCommand = new AsyncRelayCommand(OnRegisterAsync);
    }

    private async Task OnRegisterAsync()
    {
        try
        {
            _logger.Debug($"Attempting to register client: {Username}, {Email}");
            
            // Validate all inputs
            bool hasValidationErrors = false;
            
            if (string.IsNullOrWhiteSpace(Name))
            {
                NameHasError = true;
                hasValidationErrors = true;
            }
            
            if (string.IsNullOrWhiteSpace(Email) || !IsValidEmail(Email))
            {
                EmailHasError = true;
                hasValidationErrors = true;
            }
            
            if (string.IsNullOrWhiteSpace(Username))
            {
                UsernameHasError = true;
                hasValidationErrors = true;
            }
            
            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 4)
            {
                PasswordHasError = true;
                hasValidationErrors = true;
            }
            
            if (string.IsNullOrWhiteSpace(Phone) || !IsValidPhoneNumber(Phone))
            {
                PhoneHasError = true;
                hasValidationErrors = true;
            }
            
            if (hasValidationErrors)
            {
                ErrorMessage = "Please fill in all fields correctly";
                _logger.Warn("Registration attempt with invalid fields");
                return;
            }
            
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            
            // Register user
            var user = await _service.RegisterClient(Name, Email, Username, Password, Phone);
            if (user != null)
            {
                _logger.Info($"Successfully registered client: {Username}");
                SuccessMessage = $"Welcome {Name}! Your account has been created successfully.";
                
                // Show success message for a brief moment (2-3 seconds)
                await Task.Delay(2500); // 2.5 seconds delay
                
                // Registration successful - go back to login
                var currentWindow = GetCurrentWindow();
                if (currentWindow?.DataContext is LoginViewModel loginViewModel)
                {
                    loginViewModel.CurrentView = new LoginClientView { DataContext = new LoginClientViewModel() };
                }
            }
            else
            {
                ErrorMessage = "Registration failed";
                _logger.Warn($"Failed to register client: {Username}");
                // await ShowMessageBoxAsync("Registration Failed", 
                //     "Registration failed. Please try again or contact support.");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Registration error: {ex.Message}";
            _logger.Error($"Registration error: {ex.Message}", ex);
            // await ShowMessageBoxAsync("Registration Error", 
            //     $"An error occurred during registration: {ex.Message}");
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
    
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    
    private bool IsValidPhoneNumber(string phone)
    {
        return phone.Length == 10 && phone.All(char.IsDigit);
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
    //                 Width = 400,
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
