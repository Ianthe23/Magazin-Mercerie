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

public partial class RegisterAngajatViewModel : ViewModelBase
{
    private readonly IService _service;
    private string _name = string.Empty;
    private string _email = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _phone = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading = false;
    private TipAngajat _selectedTipAngajat;
    private List<TipAngajat> _tipAngajatOptions;

    public string Name 
    { 
        get => _name; 
        set 
        { 
            _name = value; 
            OnPropertyChanged(); 
        } 
    }
    
    public string Email 
    { 
        get => _email; 
        set 
        { 
            _email = value; 
            OnPropertyChanged(); 
        } 
    }
    
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
    
    public string Phone 
    { 
        get => _phone; 
        set 
        { 
            _phone = value; 
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
        RegisterCommand = new AsyncRelayCommand(OnRegisterAsync);
        TipAngajatOptions = Enum.GetValues(typeof(TipAngajat)).Cast<TipAngajat>().ToList();
        SelectedTipAngajat = TipAngajatOptions.FirstOrDefault();
    }

    private async Task OnRegisterAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            // Validate input
            if (string.IsNullOrWhiteSpace(Name) || 
                string.IsNullOrWhiteSpace(Email) || 
                string.IsNullOrWhiteSpace(Username) || 
                string.IsNullOrWhiteSpace(Password) || 
                string.IsNullOrWhiteSpace(Phone))
            {
                ErrorMessage = "All fields are required";
                return;
            }
            
            // Register user
            var user = await _service.RegisterAngajat(Name, Email, Username, Password, Phone);
            if (user != null)
            {
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
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Registration error: {ex.Message}";
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
