namespace magazin_mercerie.ViewModels;

using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using magazin_mercerie.Views.LoginViews;
using Avalonia.Controls;
using Windows = Avalonia.Controls.Window;
using log4net;

public partial class RegisterAngajatViewModel : ViewModelBase
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
    private TipAngajat _selectedTipAngajat;
    private List<TipAngajat> _tipAngajatOptions;

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
    
    public List<TipAngajat> TipAngajatOptions
    {
        get => _tipAngajatOptions;
        set
        {
            _tipAngajatOptions = value;
            OnPropertyChanged();
        }
    }
    
    public TipAngajat SelectedTipAngajat 
    { 
        get => _selectedTipAngajat; 
        set 
        { 
            _selectedTipAngajat = value; 
            OnPropertyChanged(); 
        } 
    }
    
    public ICommand RegisterCommand { get; }

    public RegisterAngajatViewModel()
    {
        _service = App.ServiceProvider.GetService(typeof(IService)) as IService;
        _logger = LogManager.GetLogger(typeof(RegisterAngajatViewModel));
        _logger.Debug("RegisterAngajatViewModel initialized");
        RegisterCommand = new AsyncRelayCommand(OnRegisterAsync);
        TipAngajatOptions = Enum.GetValues(typeof(TipAngajat)).Cast<TipAngajat>().ToList();
        SelectedTipAngajat = TipAngajatOptions.FirstOrDefault();
    }

    private async Task OnRegisterAsync()
    {
        try
        {
            _logger.Debug($"Attempting to register employee: {Username}, {Email}");
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

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

            // Register user based on selected type
            var user = await _service.RegisterAngajat(Name, Email, Username, Password, Phone, SelectedTipAngajat);
            
            if (user != null)
            {
                _logger.Info($"Successfully registered employee: {Username}");
                SuccessMessage = $"Welcome {Name}! Your employee account has been created successfully.";
                
                // Show success message for a brief moment (2-3 seconds)
                await Task.Delay(2500); // 2.5 seconds delay
                
                // Registration successful - go back to login
                var currentWindow = GetCurrentWindow();
                if (currentWindow?.DataContext is LoginViewModel loginViewModel)
                {
                    loginViewModel.CurrentView = new LoginAngajatView { DataContext = new LoginAngajatViewModel() };
                }
            }
            else
            {
                ErrorMessage = "Registration failed";
                _logger.Warn($"Failed to register employee: {Username}");
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
    
    private Windows GetCurrentWindow()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }
}
