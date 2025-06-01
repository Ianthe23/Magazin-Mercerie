using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using log4net;
using magazin_mercerie.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using magazin_mercerie.Service;
using System.Threading.Tasks;
using System.ComponentModel;

namespace magazin_mercerie.Views.ClientViews;

public partial class MainWindow : Window
{
    private readonly ILog _logger = LogManager.GetLogger(typeof(MainWindow));
    private readonly ICartService _cartService;
    private readonly IUserSessionService? _userSessionService;
    private MainViewModel? _viewModel;
    
    public MainWindow()
    {
        try
        {
            _logger.Debug("MainWindow constructor called");
            Console.WriteLine("MainWindow constructor called");
            
            InitializeComponent();
            InitializeViewModel();
            
            // Initialize cart service
            _cartService = App.ServiceProvider?.GetService<ICartService>();
            _userSessionService = App.ServiceProvider?.GetService<IUserSessionService>();
            
            // Subscribe to window closing event
            Closing += OnWindowClosing;
            
            _logger.Debug("MainWindow initialized successfully");
            Console.WriteLine("MainWindow initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in MainWindow constructor: {ex.Message}");
            _logger.Error("Error in MainWindow constructor", ex);
            throw; // Re-throw to ensure the error isn't silently caught
        }
    }

    private void InitializeComponent()
    {
        try
        {
            _logger.Debug("InitializeComponent called");
            AvaloniaXamlLoader.Load(this);
            _logger.Debug("XAML loaded successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR loading XAML: {ex.Message}");
            _logger.Error("Error loading XAML", ex);
            throw; // Re-throw to ensure the error isn't silently caught
        }
    }

    private void InitializeViewModel()
    {
        try
        {
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            _logger.Debug("MainViewModel set as DataContext");
        }
        catch (Exception ex)
        {
            _logger.Error("Error initializing MainViewModel", ex);
        }
    }
    
    private void ToggleSplitView(object sender, RoutedEventArgs e)
    {
        try
        {
            var splitView = this.FindControl<SplitView>("MainSplitView");
            if (splitView != null)
            {
                splitView.IsPaneOpen = !splitView.IsPaneOpen;
                _logger.Debug($"SplitView pane toggled to {splitView.IsPaneOpen}");
            }
            else
            {
                _logger.Warn("Could not find MainSplitView control");
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Error toggling split view", ex);
        }
    }
    
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        _logger.Info("MainWindow opened");
        Console.WriteLine("MainWindow opened successfully");
    }
    
    private async void OpenCart(object sender, RoutedEventArgs e)
    {
        try
        {
            _logger.Debug("Opening cart dialog");
            var cartDialog = new CartDialog()
            {
                DataContext = new CartViewModel()
            };
            await cartDialog.ShowDialog(this);
        }
        catch (Exception ex)
        {
            _logger.Error("Error opening cart dialog", ex);
        }
    }
    
    // This will be called when user tries to logout
    public async Task<bool> ConfirmExit()
    {
        try
        {
            if (_cartService?.HasItems == true)
            {
                _logger.Debug("Cart has items, showing confirmation dialog");
                
                // Show confirmation dialog asking if user wants to place order or clear cart
                var confirmDialog = new Window()
                {
                    Title = "Items in Cart",
                    Width = 400,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                
                var stackPanel = new StackPanel()
                {
                    Margin = new Avalonia.Thickness(20),
                    Spacing = 15
                };
                
                stackPanel.Children.Add(new TextBlock()
                {
                    Text = "You have items in your cart. What would you like to do?",
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    FontSize = 14
                });
                
                var buttonPanel = new StackPanel()
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Spacing = 10
                };
                
                var placeOrderButton = new Button()
                {
                    Content = "Place Order",
                    Background = Avalonia.Media.Brushes.Green,
                    Foreground = Avalonia.Media.Brushes.White,
                    Padding = new Avalonia.Thickness(15, 8)
                };
                
                var clearCartButton = new Button()
                {
                    Content = "Clear Cart & Exit",
                    Background = Avalonia.Media.Brushes.Red,
                    Foreground = Avalonia.Media.Brushes.White,
                    Padding = new Avalonia.Thickness(15, 8)
                };
                
                var cancelButton = new Button()
                {
                    Content = "Cancel",
                    Background = Avalonia.Media.Brushes.Gray,
                    Foreground = Avalonia.Media.Brushes.White,
                    Padding = new Avalonia.Thickness(15, 8)
                };
                
                bool? result = null;
                
                placeOrderButton.Click += (s, e) => {
                    _logger.Info("User chose to place order");
                    // TODO: Implement order placement logic
                    _cartService?.ClearCart();
                    result = true;
                    confirmDialog.Close();
                };
                
                clearCartButton.Click += (s, e) => {
                    _logger.Info("User chose to clear cart and exit");
                    _cartService?.ClearCart();
                    result = true;
                    confirmDialog.Close();
                };
                
                cancelButton.Click += (s, e) => {
                    _logger.Info("User chose to cancel exit");
                    result = false;
                    confirmDialog.Close();
                };
                
                buttonPanel.Children.Add(placeOrderButton);
                buttonPanel.Children.Add(clearCartButton);
                buttonPanel.Children.Add(cancelButton);
                
                stackPanel.Children.Add(buttonPanel);
                confirmDialog.Content = stackPanel;
                
                await confirmDialog.ShowDialog(this);
                
                return result == true;
            }
            
            _logger.Debug("Cart is empty, OK to exit");
            return true; // Cart is empty, OK to exit
        }
        catch (Exception ex)
        {
            _logger.Error("Error in ConfirmExit", ex);
            return true; // Allow exit on error
        }
    }

    private void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        try
        {
            _logger?.Info("🚪 MainWindow closing - cleaning up session...");
            
            // Clear the window user from UserSessionService
            if (_userSessionService != null)
            {
                _userSessionService.ClearWindowUser("MainWindow");
                _logger?.Info("✅ Cleared MainWindow user from session service");
            }
            
            // Dispose the ViewModel if it implements IDisposable
            if (_viewModel is IDisposable disposableViewModel)
            {
                disposableViewModel.Dispose();
                _logger?.Debug("MainViewModel disposed");
            }
        }
        catch (Exception ex)
        {
            _logger?.Error("Error during MainWindow cleanup", ex);
        }
    }
}
