using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using log4net;
using magazin_mercerie.ViewModels;
using magazin_mercerie.Service;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;

namespace magazin_mercerie.Views.PatronViews
{
    public partial class PatronWindow : Window
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(PatronWindow));
        private readonly IUserSessionService? _userSessionService;
        private PatronViewModel? _viewModel;

        public PatronWindow()
        {
            InitializeComponent();
            InitializeViewModel();
            
            _userSessionService = App.ServiceProvider?.GetService<IUserSessionService>();
            
            // Subscribe to window closing event
            Closing += OnWindowClosing;
            
            _logger.Info("PatronWindow initialized");
        }

        private void OnWindowClosing(object? sender, WindowClosingEventArgs e)
        {
            try
            {
                _logger?.Info("🚪 PatronWindow closing - cleaning up session...");
                
                // Clear the window user from UserSessionService if this was a patron window
                if (_userSessionService != null)
                {
                    _userSessionService.ClearWindowUser("PatronWindow");
                    _logger?.Info("✅ Cleared PatronWindow user from session service");
                }
                
                // Dispose the ViewModel if it implements IDisposable
                if (_viewModel is IDisposable disposableViewModel)
                {
                    disposableViewModel.Dispose();
                    _logger?.Debug("PatronViewModel disposed");
                }
            }
            catch (Exception ex)
            {
                _logger?.Error("Error during PatronWindow cleanup", ex);
            }
        }

        private void InitializeViewModel()
        {
            try
            {
                _viewModel = new PatronViewModel();
                DataContext = _viewModel;
                _logger.Debug("PatronViewModel set as DataContext");
            }
            catch (Exception ex)
            {
                _logger.Error("Error initializing PatronViewModel", ex);
            }
        }

        private void ToggleSplitView(object? sender, RoutedEventArgs e)
        {
            var splitView = this.FindControl<SplitView>("MainSplitView");
            if (splitView != null)
            {
                splitView.IsPaneOpen = !splitView.IsPaneOpen;
            }
        }

        private void ShowEmployeeManagement(object? sender, RoutedEventArgs e)
        {
            _logger.Debug("Switching to Employee Management panel");
            
            // Hide all panels
            HideAllPanels();
            
            // Show employee management panel
            var employeePanel = this.FindControl<Grid>("EmployeeManagementPanel");
            if (employeePanel != null)
            {
                employeePanel.IsVisible = true;
            }
            
            // Update button styles
            UpdateButtonStyles("EmployeesButton");
            
            // Refresh employee data
            _ = _viewModel?.LoadEmployeesAsync();
        }

        private void ShowProductManagement(object? sender, RoutedEventArgs e)
        {
            _logger.Debug("Switching to Product Management panel");
            
            // Hide all panels
            HideAllPanels();
            
            // Show product management panel
            var productPanel = this.FindControl<Grid>("ProductManagementPanel");
            if (productPanel != null)
            {
                productPanel.IsVisible = true;
            }
            
            // Update button styles
            UpdateButtonStyles("ProductsButton");
            
            // Refresh product data
            _ = _viewModel?.LoadProductsAsync();
        }

        private void ShowReports(object? sender, RoutedEventArgs e)
        {
            _logger.Debug("Switching to Reports panel");
            
            // Hide all panels
            HideAllPanels();
            
            // TODO: Show reports panel when implemented
            // For now, show employee management
            var employeePanel = this.FindControl<Grid>("EmployeeManagementPanel");
            if (employeePanel != null)
            {
                employeePanel.IsVisible = true;
            }
            
            // Update button styles
            UpdateButtonStyles("ReportsButton");
        }

        private void ShowOrdersOverview(object? sender, RoutedEventArgs e)
        {
            _logger.Debug("Switching to Orders Overview panel");
            
            // Hide all panels
            HideAllPanels();
            
            // TODO: Show orders overview panel when implemented
            // For now, show employee management
            var employeePanel = this.FindControl<Grid>("EmployeeManagementPanel");
            if (employeePanel != null)
            {
                employeePanel.IsVisible = true;
            }
            
            // Update button styles
            UpdateButtonStyles("OrdersButton");
        }

        private void HideAllPanels()
        {
            var employeePanel = this.FindControl<Grid>("EmployeeManagementPanel");
            var productPanel = this.FindControl<Grid>("ProductManagementPanel");
            
            if (employeePanel != null) employeePanel.IsVisible = false;
            if (productPanel != null) productPanel.IsVisible = false;
        }

        private void UpdateButtonStyles(string activeButtonName)
        {
            // Reset all button styles
            var buttons = new[] { "EmployeesButton", "ProductsButton", "ReportsButton", "OrdersButton" };
            
            foreach (var buttonName in buttons)
            {
                var button = this.FindControl<Button>(buttonName);
                if (button != null)
                {
                    if (buttonName == activeButtonName)
                    {
                        // Active button style
                        button.Background = new SolidColorBrush(Color.Parse("#4361ee"));
                        button.Foreground = new SolidColorBrush(Colors.White);
                        var textBlock = button.Content as StackPanel;
                        if (textBlock?.Children[1] is TextBlock tb)
                        {
                            tb.FontWeight = FontWeight.SemiBold;
                        }
                    }
                    else
                    {
                        // Inactive button style
                        button.Background = new SolidColorBrush(Colors.Transparent);
                        button.Foreground = new SolidColorBrush(Colors.White);
                        var textBlock = button.Content as StackPanel;
                        if (textBlock?.Children[1] is TextBlock tb)
                        {
                            tb.FontWeight = FontWeight.Normal;
                        }
                    }
                }
            }
        }

        // Employee Management Actions
        private void AddNewEmployee(object? sender, RoutedEventArgs e)
        {
            _logger.Info("Add New Employee button clicked");
            _viewModel?.AddEmployeeCommand.Execute(null);
        }

        // Product Management Actions
        private void AddNewProduct(object? sender, RoutedEventArgs e)
        {
            _logger.Info("Add New Product button clicked");
            _viewModel?.AddProductCommand.Execute(null);
        }

        // General Actions
        private async void Logout(object? sender, RoutedEventArgs e)
        {
            _logger.Info("Patron logout requested");
            
            try
            {
                // Create and show login window first
                var loginWindow = new magazin_mercerie.Views.LoginViews.LoginWindow()
                {
                    DataContext = new LoginViewModel()
                };
                loginWindow.Show();
                
                // Set the login window as the new MainWindow to prevent app termination
                if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.MainWindow = loginWindow;
                }
                
                _logger.Debug("Login window created and set as MainWindow");
                
                // Clear user session
                var sessionService = magazin_mercerie.App.ServiceProvider?.GetService(typeof(magazin_mercerie.Service.IUserSessionService)) as magazin_mercerie.Service.IUserSessionService;
                sessionService?.ClearCurrentUser();
                
                // Close this patron window
                this.Close();
                
                _logger.Info("Patron logout completed successfully");
            }
            catch (Exception ex)
            {
                _logger.Error("Error during patron logout", ex);
                // Fallback - just close the window
                this.Close();
            }
        }

        // Modal overlay methods
        public void ShowModalOverlay()
        {
            var overlay = this.FindControl<Border>("ModalOverlay");
            if (overlay != null)
            {
                overlay.IsVisible = true;
                _logger.Debug("Modal overlay shown");
            }
        }

        public void HideModalOverlay()
        {
            var overlay = this.FindControl<Border>("ModalOverlay");
            if (overlay != null)
            {
                overlay.IsVisible = false;
                _logger.Debug("Modal overlay hidden");
            }
        }

        private void CategoryFilterChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && 
                comboBox.SelectedItem is ComboBoxItem selectedItem &&
                DataContext is PatronViewModel viewModel)
            {
                var selectedCategory = selectedItem.Tag?.ToString() ?? "All";
                viewModel.SelectedCategory = selectedCategory;
            }
        }
    }
} 