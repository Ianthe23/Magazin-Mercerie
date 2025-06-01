using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using magazin_mercerie.Service;
using System.Collections.Generic;
using Avalonia.Controls;

namespace magazin_mercerie.ViewModels
{
    public class AngajatViewModel : ViewModelBase
    {
        private readonly ILog _logger;
        private readonly IService _service;
        private readonly IUserSessionService _userSessionService;
        private readonly IProductUpdateNotificationService _notificationService;
        
        // Employee ID for this specific window
        private Guid? _employeeId;
        
        // Observable collections for data binding
        public ObservableCollection<Comanda> AssignedOrders { get; set; }
        public ObservableCollection<Status> AvailableStatuses { get; set; }
        
        // Properties
        private bool _isLoading = false;
        private string _errorMessage = string.Empty;
        private string _currentEmployeeName = "Employee";
        private string _currentEmployeeInitial = "E";
        private int _orderCount = 0;
        
        // Pagination properties
        private int _currentPage = 1;
        private int _pageSize = 5; // 5 orders per page
        private int _totalOrders = 0;
        private int _totalPages = 0;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
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

        public string CurrentEmployeeName
        {
            get => _currentEmployeeName;
            set
            {
                _currentEmployeeName = value;
                OnPropertyChanged();
            }
        }
        
        public string CurrentEmployeeInitial
        {
            get => _currentEmployeeInitial;
            set
            {
                _currentEmployeeInitial = value;
                OnPropertyChanged();
            }
        }
        
        public int OrderCount
        {
            get => _orderCount;
            set
            {
                _orderCount = value;
                OnPropertyChanged();
            }
        }
        
        // Pagination Properties
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PageInfo));
                OnPropertyChanged(nameof(CanGoPreviousPage));
                OnPropertyChanged(nameof(CanGoNextPage));
            }
        }
        
        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (_pageSize != value)
                {
                    _pageSize = value;
                    OnPropertyChanged();
                    UpdateTotalPages();
                    
                    // Reset to page 1 when page size changes and reload
                    CurrentPage = 1;
                    _ = Task.Run(async () => await LoadOrdersAsync());
                }
            }
        }
        
        public int TotalOrders
        {
            get => _totalOrders;
            set
            {
                _totalOrders = value;
                OnPropertyChanged();
                UpdateTotalPages();
            }
        }
        
        public int TotalPages
        {
            get => _totalPages;
            set
            {
                _totalPages = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PageInfo));
                OnPropertyChanged(nameof(CanGoPreviousPage));
                OnPropertyChanged(nameof(CanGoNextPage));
            }
        }
        
        public string PageInfo => $"Page {CurrentPage} of {TotalPages} ({TotalOrders} total orders)";
        
        public bool CanGoPreviousPage => CurrentPage > 1;
        public bool CanGoNextPage => CurrentPage < TotalPages;
        public bool ShowPagination => TotalPages > 1;

        // Commands
        public ICommand LoadOrdersCommand { get; }
        public ICommand UpdateOrderStatusCommand { get; }
        public ICommand UpdateQuantitiesCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CompleteOrderCommand { get; }
        
        // Pagination Commands
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand GoToPageCommand { get; }

        public AngajatViewModel()
        {
            try
            {
                _service = App.ServiceProvider?.GetService<IService>();
                _logger = LogManager.GetLogger(typeof(AngajatViewModel));
                _userSessionService = App.ServiceProvider?.GetService<IUserSessionService>();
                _notificationService = App.ServiceProvider?.GetService<IProductUpdateNotificationService>();
                
                // Initialize collections
                AssignedOrders = new ObservableCollection<Comanda>();
                AvailableStatuses = new ObservableCollection<Status>
                {
                    Status.Preluat,
                    Status.Procesat,
                    Status.Finalizat
                };
                
                // Initialize commands
                LoadOrdersCommand = new AsyncRelayCommand(LoadOrdersAsync);
                UpdateOrderStatusCommand = new AsyncRelayCommand<Comanda>(UpdateOrderStatusAsync);
                UpdateQuantitiesCommand = new AsyncRelayCommand<Comanda>(UpdateQuantitiesAsync);
                LogoutCommand = new AsyncRelayCommand(LogoutAsync);
                RefreshCommand = new AsyncRelayCommand(LoadOrdersAsync);
                CompleteOrderCommand = new AsyncRelayCommand<object>(CompleteOrderAsync);
                
                // Pagination commands
                NextPageCommand = new AsyncRelayCommand(NextPageAsync);
                PreviousPageCommand = new AsyncRelayCommand(PreviousPageAsync);
                GoToPageCommand = new AsyncRelayCommand<int>(GoToPageAsync);
                
                _logger?.Debug("Commands initialized successfully");
                _logger?.Debug($"UpdateQuantitiesCommand created: {UpdateQuantitiesCommand != null}");
                
                // Initialize employee data
                InitializeEmployeeData();
                
                // Subscribe to product update notifications
                if (_notificationService != null)
                {
                    _logger?.Info($"🔗 AngajatViewModel SUBSCRIBING to product notifications on service {_notificationService.GetHashCode()}...");
                    _notificationService.ProductQuantityUpdated += OnProductQuantityUpdated;
                    _logger?.Info($"✅ AngajatViewModel SUBSCRIBED to product update notifications");
                }
                else
                {
                    _logger?.Error($"❌ NOTIFICATION SERVICE NOT FOUND in AngajatViewModel - Real-time updates will not work!");
                }
                
                _logger?.Info("AngajatViewModel initialized");
                
                // Load initial data
                _ = Task.Run(async () => await LoadOrdersAsync());
            }
            catch (Exception ex)
            {
                _logger?.Error("Error initializing AngajatViewModel", ex);
                ErrorMessage = $"Error initializing: {ex.Message}";
            }
        }
        
        // Set the employee ID for this specific AngajatWindow
        public void SetEmployeeId(Guid employeeId)
        {
            _employeeId = employeeId;
            _logger?.Debug($"AngajatViewModel set to employee ID: {employeeId}");
            
            // Refresh employee data with the new ID
            InitializeEmployeeData();
        }
        
        private void InitializeEmployeeData()
        {
            Angajat? currentEmployee = null;
            
            // Try to get employee using the specific employee ID first
            if (_employeeId.HasValue)
            {
                currentEmployee = _userSessionService?.GetEmployeeForAngajatWindow(_employeeId.Value);
                _logger?.Debug($"Retrieved employee from session using ID {_employeeId}: {(currentEmployee?.Username ?? "null")}");
            }
            
            // Fallback approaches if the specific ID lookup failed
            if (currentEmployee == null)
            {
                _logger?.Warn("Employee not found using specific ID - trying fallbacks");
                
                // Try the old generic window lookup
                currentEmployee = _userSessionService?.GetEmployeeForWindow("AngajatWindow");
                if (currentEmployee != null)
                {
                    _logger?.Debug($"Found employee using generic window lookup: {currentEmployee.Username}");
                }
                else
                {
                    // Final fallback to global current employee
                    currentEmployee = _userSessionService?.GetCurrentEmployee();
                    if (currentEmployee != null)
                    {
                        _logger?.Debug($"Using global current employee: {currentEmployee.Username}");
                    }
                }
            }
            
            if (currentEmployee != null)
            {
                CurrentEmployeeName = currentEmployee.Nume;
                CurrentEmployeeInitial = !string.IsNullOrEmpty(currentEmployee.Nume) ? 
                    currentEmployee.Nume.Substring(0, 1).ToUpper() : "E";
                    
                _logger?.Debug($"Initialized AngajatWindow with employee: {currentEmployee.Username} (ID: {currentEmployee.Id})");
            }
            else
            {
                _logger?.Error("❌ CRITICAL: No employee found for AngajatWindow with any lookup method");
                CurrentEmployeeName = "Unknown Employee";
                CurrentEmployeeInitial = "?";
            }
        }

        public async Task LoadOrdersAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                
                Angajat? currentEmployee = null;
                
                // Try to get employee using the specific employee ID first
                if (_employeeId.HasValue)
                {
                    currentEmployee = _userSessionService?.GetEmployeeForAngajatWindow(_employeeId.Value);
                    _logger?.Debug($"Retrieved employee for orders using ID {_employeeId}: {(currentEmployee?.Username ?? "null")}");
                }
                
                // Fallback approaches if the specific ID lookup failed
                if (currentEmployee == null)
                {
                    _logger?.Warn("Employee not found using specific ID for orders - trying fallbacks");
                    
                    // Try the old generic window lookup
                    currentEmployee = _userSessionService?.GetEmployeeForWindow("AngajatWindow");
                    if (currentEmployee == null)
                    {
                        // Final fallback to global current employee
                        currentEmployee = _userSessionService?.GetCurrentEmployee();
                    }
                }
                
                if (currentEmployee == null)
                {
                    ErrorMessage = "No employee session found. Please log in.";
                    _logger?.Error("❌ CRITICAL: No employee found for loading orders with any lookup method");
                    return;
                }
                
                _logger?.Debug($"Loading orders for employee: {currentEmployee.Username} (ID: {currentEmployee.Id}) (Page {CurrentPage}, Size {PageSize})");
                
                // Use paginated service method
                var (orders, totalCount) = await _service.AfisareComenziAngajat(currentEmployee.Id, CurrentPage, PageSize);
                
                // Update UI on main thread
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    AssignedOrders.Clear();
                    foreach (var order in orders)
                    {
                        AssignedOrders.Add(order);
                    }
                    
                    // Update pagination info
                    TotalOrders = totalCount;
                    OrderCount = AssignedOrders.Count; // Current page count
                });
                
                _logger?.Info($"✅ Loaded page {CurrentPage}: {orders.Count} orders of {totalCount} total for employee {currentEmployee.Username} (ID: {currentEmployee.Id})");
            }
            catch (Exception ex)
            {
                _logger?.Error("Error loading employee orders", ex);
                ErrorMessage = $"Error loading orders: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UpdateOrderStatusAsync(Comanda order)
        {
            try
            {
                if (order == null)
                {
                    _logger.Warn("Invalid parameters for UpdateOrderStatus");
                    return;
                }

                var orderId = order.Id;
                var newStatus = order.Status;

                _logger.Debug($"Updating order {orderId} to status {newStatus}");

                var updatedOrder = await _service.ActualizareStatusComanda(orderId, newStatus);
                if (updatedOrder != null)
                {
                    _logger.Info($"Order {orderId} status updated to {newStatus}");
                    
                    // CRITICAL FIX: Force UI refresh by reloading orders
                    // This ensures the UI sees the status changes immediately
                    _logger.Info($"🔄 FORCING UI REFRESH: Reloading orders after status update...");
                    await LoadOrdersAsync();
                    _logger.Info($"✅ Orders reloaded - UI should now show updated status: {newStatus}");
                }
                else
                {
                    _logger.Error($"Failed to update order {orderId}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error updating order status: {ex.Message}");
            }
        }

        private async Task CompleteOrderAsync(object parameter)
        {
            try
            {
                if (parameter is Guid orderId)
                {
                    _logger.Debug($"Completing order {orderId}");

                    var updatedOrder = await _service.ActualizareStatusComanda(orderId, Status.Finalizat);
                    if (updatedOrder != null)
                    {
                        _logger.Info($"Order {orderId} completed successfully");
                        
                        // CRITICAL FIX: Force UI refresh by reloading orders
                        // This ensures the UI sees the status changes immediately
                        _logger.Info($"🔄 FORCING UI REFRESH: Reloading orders after completion...");
                        await LoadOrdersAsync();
                        _logger.Info($"✅ Orders reloaded - UI should now show completed status");
                    }
                    else
                    {
                        _logger.Error($"Failed to complete order {orderId}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error completing order: {ex.Message}");
            }
        }

        private async Task UpdateQuantitiesAsync(Comanda order)
        {
            _logger?.Debug("=== UpdateQuantitiesAsync called ===");
            _logger?.Debug($"Order parameter: {order?.Id ?? Guid.Empty}");
            _logger?.Debug($"Order is null: {order == null}");
            
            try
            {
                if (order?.ComandaProduse == null || !order.ComandaProduse.Any()) 
                {
                    _logger?.Debug($"Early return - ComandaProduse is null or empty. Count: {order?.ComandaProduse?.Count ?? -1}");
                    return;
                }

                _logger?.Debug($"Starting quantity update for order {order.Id}");

                // Create a dictionary of current product quantities from the UI (updated values)
                var productQuantities = new Dictionary<Guid, decimal>();
                foreach (var comandaProdus in order.ComandaProduse)
                {
                    // Use the current stock quantity from the product (which should be updated in the UI)
                    var currentQuantity = comandaProdus.Produs.Cantitate;
                    productQuantities[comandaProdus.ProdusId] = currentQuantity;
                    
                    _logger?.Debug($"Product {comandaProdus.Produs.Nume} (ID: {comandaProdus.ProdusId}) - setting quantity to {currentQuantity}");
                }

                _logger?.Debug($"Calling service to update quantities for {productQuantities.Count} products");
                var success = await _service.ActualizareCantitateProduseComanda(order.Id, productQuantities);
                if (success)
                {
                    _logger?.Info($"✅ SERVICE CALL SUCCESSFUL - Product quantities updated for order {order.Id}");
                    _logger?.Info($"🔔 Notifications should have been triggered for {productQuantities.Count} products");
                    
                    await ShowSuccessNotificationAsync("Quantities Updated", 
                        $"Product quantities updated for order {order.Id}");
                    
                    _logger?.Info($"Successfully updated quantities for order {order.Id}");
                }
                else
                {
                    _logger?.Error($"❌ SERVICE CALL FAILED - Could not update quantities for order {order.Id}");
                    await ShowErrorNotificationAsync("Update Failed", "Failed to update product quantities.");
                }
            }
            catch (Exception ex)
            {
                _logger?.Error("Error updating quantities", ex);
                await ShowErrorNotificationAsync("Error", $"Error updating quantities: {ex.Message}");
            }
        }

        private async Task RefreshOrdersAsync()
        {
            await LoadOrdersAsync();
        }

        private async Task LogoutAsync()
        {
            try
            {
                // Clear ONLY this employee's session, not the global current user
                if (_employeeId.HasValue)
                {
                    _userSessionService?.ClearEmployeeForAngajatWindow(_employeeId.Value);
                    _logger?.Debug($"Cleared session for employee {_employeeId} only");
                }
                else
                {
                    // Fallback: if no specific employee ID, try to clear a generic session
                    _userSessionService?.ClearWindowUser("AngajatWindow");
                    _logger?.Debug("Cleared generic AngajatWindow session (fallback)");
                }
                
                // DON'T clear the global current user - this would affect other employees
                // _userSessionService?.ClearCurrentUser(); // REMOVED - this was causing cross-logout
                
                // Navigate back to login (reuse existing window)
                await NavigateToLoginAsync();
            }
            catch (Exception ex)
            {
                _logger?.Error("Error during logout", ex);
            }
        }

        private async void OnProductQuantityUpdated(object sender, ProductQuantityUpdatedEventArgs e)
        {
            try
            {
                _logger?.Info($"🔔 AngajatViewModel NOTIFICATION RECEIVED: Product {e.ProductId} quantity updated to {e.NewQuantity}");
                
                // Update any order displays that might show product quantities
                // For now, just refresh the orders to get the latest data
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    _logger?.Info($"🔄 AngajatViewModel refreshing orders due to product quantity update");
                    await LoadOrdersAsync();
                });
            }
            catch (Exception ex)
            {
                _logger?.Error("❌ AngajatViewModel error handling product quantity update notification", ex);
            }
        }

        private async Task ShowSuccessNotificationAsync(string title, string message)
        {
            try
            {
                // Find the parent window (AngajatWindow)
                Window parentWindow = null;
                if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                {
                    parentWindow = desktop.Windows.FirstOrDefault(w => w.Title == "Merceria - Employee Dashboard" || w.GetType().Name.Contains("AngajatWindow"));
                }

                var notificationWindow = new Window()
                {
                    Title = title,
                    Width = 450,
                    Height = 200,
                    Topmost = true,
                    CanResize = false,
                    ShowInTaskbar = false,
                    SystemDecorations = Avalonia.Controls.SystemDecorations.BorderOnly,
                    Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(35, 36, 42)) // #23242a
                };

                // Position relative to parent window (employee view)
                if (parentWindow != null)
                {
                    notificationWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                    var parentBounds = parentWindow.Bounds;
                    notificationWindow.Position = new Avalonia.PixelPoint(
                        (int)(parentBounds.X + (parentBounds.Width - 450) / 2),
                        (int)(parentBounds.Y + 100) // Offset from top of parent
                    );
                }
                else
                {
                    notificationWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }

                var stackPanel = new StackPanel()
                {
                    Margin = new Avalonia.Thickness(20),
                    Spacing = 15,
                    Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(35, 36, 42)) // #23242a
                };

                stackPanel.Children.Add(new TextBlock()
                {
                    Text = "✅ Success",
                    FontSize = 18,
                    FontWeight = Avalonia.Media.FontWeight.Bold,
                    Foreground = Avalonia.Media.Brushes.Green,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Margin = new Avalonia.Thickness(0, 0, 0, 10)
                });

                stackPanel.Children.Add(new TextBlock()
                {
                    Text = message,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    FontSize = 14,
                    Foreground = Avalonia.Media.Brushes.White, // White text on dark background
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    TextAlignment = Avalonia.Media.TextAlignment.Center
                });

                var okButton = new Button()
                {
                    Content = "OK",
                    Background = Avalonia.Media.Brushes.Green,
                    Foreground = Avalonia.Media.Brushes.White,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Padding = new Avalonia.Thickness(30, 8),
                    FontWeight = Avalonia.Media.FontWeight.Bold,
                    Margin = new Avalonia.Thickness(0, 10, 0, 0)
                };

                okButton.Click += (s, e) => notificationWindow.Close();
                stackPanel.Children.Add(okButton);
                notificationWindow.Content = stackPanel;

                notificationWindow.Show();

                // Auto-close after 3 seconds
                await Task.Delay(3000);
                if (notificationWindow.IsVisible)
                {
                    notificationWindow.Close();
                }
            }
            catch (Exception ex)
            {
                _logger?.Error("Error showing success notification", ex);
            }
        }

        private async Task ShowErrorNotificationAsync(string title, string message)
        {
            try
            {
                // Find the parent window (AngajatWindow)
                Window parentWindow = null;
                if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                {
                    parentWindow = desktop.Windows.FirstOrDefault(w => w.Title == "Merceria - Employee Dashboard" || w.GetType().Name.Contains("AngajatWindow"));
                }

                var notificationWindow = new Window()
                {
                    Title = title,
                    Width = 450,
                    Height = 200,
                    Topmost = true,
                    CanResize = false,
                    ShowInTaskbar = false,
                    SystemDecorations = Avalonia.Controls.SystemDecorations.BorderOnly,
                    Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(35, 36, 42)) // #23242a
                };

                // Position relative to parent window (employee view)
                if (parentWindow != null)
                {
                    notificationWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                    var parentBounds = parentWindow.Bounds;
                    notificationWindow.Position = new Avalonia.PixelPoint(
                        (int)(parentBounds.X + (parentBounds.Width - 450) / 2),
                        (int)(parentBounds.Y + 100) // Offset from top of parent
                    );
                }
                else
                {
                    notificationWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }

                var stackPanel = new StackPanel()
                {
                    Margin = new Avalonia.Thickness(20),
                    Spacing = 15,
                    Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(35, 36, 42)) // #23242a
                };

                stackPanel.Children.Add(new TextBlock()
                {
                    Text = "❌ Error",
                    FontSize = 18,
                    FontWeight = Avalonia.Media.FontWeight.Bold,
                    Foreground = Avalonia.Media.Brushes.Red,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Margin = new Avalonia.Thickness(0, 0, 0, 10)
                });

                stackPanel.Children.Add(new TextBlock()
                {
                    Text = message,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    FontSize = 14,
                    Foreground = Avalonia.Media.Brushes.White, // White text on dark background
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    TextAlignment = Avalonia.Media.TextAlignment.Center
                });

                var okButton = new Button()
                {
                    Content = "OK",
                    Background = Avalonia.Media.Brushes.Red,
                    Foreground = Avalonia.Media.Brushes.White,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Padding = new Avalonia.Thickness(30, 8),
                    FontWeight = Avalonia.Media.FontWeight.Bold,
                    Margin = new Avalonia.Thickness(0, 10, 0, 0)
                };

                okButton.Click += (s, e) => notificationWindow.Close();
                stackPanel.Children.Add(okButton);
                notificationWindow.Content = stackPanel;

                notificationWindow.Show();
            }
            catch (Exception ex)
            {
                _logger?.Error("Error showing error notification", ex);
            }
        }

        private async Task NavigateToLoginAsync()
        {
            try
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    // Try to find existing login window first
                    Window existingLoginWindow = null;
                    if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        existingLoginWindow = desktop.Windows.FirstOrDefault(w => 
                            w.GetType().Name.Contains("LoginWindow") || 
                            w.Title?.Contains("Login") == true);
                    }
                    
                    if (existingLoginWindow != null)
                    {
                        // Reuse existing login window - bring it to front and activate it
                        _logger?.Debug("Reusing existing login window");
                        existingLoginWindow.Activate();
                        existingLoginWindow.WindowState = WindowState.Normal;
                        existingLoginWindow.Topmost = true;
                        existingLoginWindow.Topmost = false; // Reset topmost to allow normal behavior
                    }
                    else
                    {
                        // Only create new login window if none exists
                        _logger?.Debug("No existing login window found - creating new one");
                        var loginWindow = new magazin_mercerie.Views.LoginViews.LoginWindow
                        {
                            DataContext = new LoginViewModel()
                        };
                        loginWindow.Show();
                        
                        // Set as MainWindow to prevent app termination
                        if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktopLifetime)
                        {
                            desktopLifetime.MainWindow = loginWindow;
                        }
                    }
                    
                    // Find and close the specific AngajatWindow that initiated the logout
                    if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop2)
                    {
                        // Find the specific AngajatWindow that contains this ViewModel
                        var angajatWindow = desktop2.Windows.FirstOrDefault(w => 
                            w.GetType().Name.Contains("AngajatWindow") && 
                            w.DataContext == this);
                        
                        if (angajatWindow == null)
                        {
                            // Fallback: find by title or type
                            angajatWindow = desktop2.Windows.FirstOrDefault(w => 
                                w.GetType().Name.Contains("AngajatWindow") || 
                                w.Title?.Contains("Employee") == true);
                        }
                        
                        if (angajatWindow != null)
                        {
                            _logger?.Debug($"Closing specific AngajatWindow during logout: {angajatWindow.GetHashCode()}");
                            angajatWindow.Close();
                        }
                        else
                        {
                            _logger?.Warn("Could not find specific AngajatWindow to close during logout");
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.Error("Error navigating to login", ex);
            }
        }

        private void UpdateTotalPages()
        {
            TotalPages = TotalOrders > 0 ? (int)Math.Ceiling((double)TotalOrders / PageSize) : 0;
            OnPropertyChanged(nameof(ShowPagination));
        }
        
        // Pagination Methods
        private async Task NextPageAsync()
        {
            if (CanGoNextPage)
            {
                CurrentPage++;
                await LoadOrdersAsync();
            }
        }
        
        private async Task PreviousPageAsync()
        {
            if (CanGoPreviousPage)
            {
                CurrentPage--;
                await LoadOrdersAsync();
            }
        }
        
        private async Task GoToPageAsync(int pageNumber)
        {
            if (pageNumber >= 1 && pageNumber <= TotalPages)
            {
                CurrentPage = pageNumber;
                await LoadOrdersAsync();
            }
        }

        // Cleanup resources
        public void Dispose()
        {
            // Unsubscribe from events to prevent memory leaks
            if (_notificationService != null)
            {
                _notificationService.ProductQuantityUpdated -= OnProductQuantityUpdated;
                _logger?.Debug("AngajatViewModel unsubscribed from product notifications");
            }
            
            _logger?.Debug("AngajatViewModel disposed");
        }
    }
} 