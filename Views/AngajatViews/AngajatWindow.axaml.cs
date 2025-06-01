using Avalonia.Controls;
using Avalonia.Interactivity;
using magazin_mercerie.ViewModels;
using magazin_mercerie.Models;
using CommunityToolkit.Mvvm.Input;
using magazin_mercerie.Service;
using Microsoft.Extensions.DependencyInjection;
using log4net;
using System;
using System.ComponentModel;

namespace magazin_mercerie.Views.AngajatViews
{
    public partial class AngajatWindow : Window
    {
        private readonly ILog _logger;
        private readonly IUserSessionService? _userSessionService;

        public AngajatWindow()
        {
            InitializeComponent();
            DataContext = new AngajatViewModel();
            
            _logger = LogManager.GetLogger(typeof(AngajatWindow));
            _userSessionService = App.ServiceProvider?.GetService<IUserSessionService>();
            
            // Subscribe to window closing event
            Closing += OnWindowClosing;
            
            _logger?.Debug("AngajatWindow initialized with cleanup handlers");
        }
        
        public AngajatWindow(Guid employeeId) : this()
        {
            // Store the employee ID for session management
            this.Tag = employeeId;
            
            // Update the ViewModel with the specific employee ID
            if (DataContext is AngajatViewModel viewModel)
            {
                viewModel.SetEmployeeId(employeeId);
            }
            
            _logger?.Debug($"AngajatWindow initialized for employee ID: {employeeId}");
        }

        private void OnWindowClosing(object? sender, WindowClosingEventArgs e)
        {
            try
            {
                _logger?.Info("🚪 AngajatWindow closing - cleaning up session...");
                
                // Clear the employee-specific session using the employee ID from the Tag
                if (_userSessionService != null && this.Tag is Guid employeeId)
                {
                    _userSessionService.ClearEmployeeForAngajatWindow(employeeId);
                    _logger?.Info($"✅ Cleared employee session for AngajatWindow with ID: {employeeId}");
                }
                else
                {
                    // Fallback to the old method if Tag is not set
                    _userSessionService?.ClearWindowUser("AngajatWindow");
                    _logger?.Info("✅ Cleared AngajatWindow user from session service (fallback)");
                }
                
                // Dispose the ViewModel if it implements IDisposable
                if (DataContext is IDisposable disposableViewModel)
                {
                    disposableViewModel.Dispose();
                    _logger?.Debug("AngajatViewModel disposed");
                }
            }
            catch (Exception ex)
            {
                _logger?.Error("Error during AngajatWindow cleanup", ex);
            }
        }

        private async void UpdateStock_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && 
                button.Tag is Comanda order && 
                DataContext is AngajatViewModel viewModel &&
                viewModel.UpdateQuantitiesCommand is AsyncRelayCommand<Comanda> asyncCommand)
            {
                await asyncCommand.ExecuteAsync(order);
            }
        }
    }
} 