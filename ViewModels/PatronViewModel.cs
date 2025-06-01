using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using magazin_mercerie.Views.PatronViews;
using Avalonia.Controls;
using System.Linq;
using System.Threading;
using magazin_mercerie.Service;
using System.Collections.Generic;

namespace magazin_mercerie.ViewModels
{
    public partial class PatronViewModel : ViewModelBase
    {
        private readonly ILog _logger;
        private readonly IService _service;
        private readonly IProductUpdateNotificationService _notificationService;
        private readonly IUserSessionService _userSessionService;
        private Timer _searchTimer;
        private Timer _productSearchTimer;
        
        // Observable collections for data binding
        public ObservableCollection<Angajat> Employees { get; set; }
        public ObservableCollection<Angajat> FilteredEmployees { get; set; }
        public ObservableCollection<Produs> Products { get; set; }
        public ObservableCollection<Produs> FilteredProducts { get; set; }
        
        // Search and filter properties
        private string _employeeSearchText = string.Empty;
        private string _productSearchText = string.Empty;
        private string _selectedCategory = "All";
        private bool _isLoading = false;
        private string _errorMessage = string.Empty;

        public string EmployeeSearchText
        {
            get => _employeeSearchText;
            set
            {
                _employeeSearchText = value;
                OnPropertyChanged();
                DebounceEmployeeSearch();
            }
        }

        public string ProductSearchText
        {
            get => _productSearchText;
            set
            {
                _productSearchText = value;
                OnPropertyChanged();
                DebounceProductSearch();
            }
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                PerformProductFilterSync();
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

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        // Commands
        public ICommand LoadEmployeesCommand { get; }
        public ICommand LoadProductsCommand { get; }
        public ICommand AddEmployeeCommand { get; }
        public ICommand EditEmployeeCommand { get; }
        public ICommand DeleteEmployeeCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }

        public PatronViewModel()
        {
            try
            {
                _service = App.ServiceProvider?.GetService<IService>();
                _logger = LogManager.GetLogger(typeof(PatronViewModel));
                _notificationService = App.ServiceProvider?.GetService<IProductUpdateNotificationService>();
                _userSessionService = App.ServiceProvider?.GetService<IUserSessionService>();
                
                // Initialize collections
                Employees = new ObservableCollection<Angajat>();
                FilteredEmployees = new ObservableCollection<Angajat>();
                Products = new ObservableCollection<Produs>();
                FilteredProducts = new ObservableCollection<Produs>();
                
                // Initialize commands
                LoadEmployeesCommand = new AsyncRelayCommand(LoadEmployeesAsync);
                LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync);
                AddEmployeeCommand = new AsyncRelayCommand(AddEmployeeAsync);
                EditEmployeeCommand = new AsyncRelayCommand<Angajat>(EditEmployeeAsync);
                DeleteEmployeeCommand = new AsyncRelayCommand<Angajat>(DeleteEmployeeAsync);
                AddProductCommand = new AsyncRelayCommand(AddProductAsync);
                EditProductCommand = new AsyncRelayCommand<Produs>(EditProductAsync);
                DeleteProductCommand = new AsyncRelayCommand<Produs>(DeleteProductAsync);
                
                // Subscribe to product update notifications
                if (_notificationService != null)
                {
                    _logger?.Info($"🔗 PatronViewModel SUBSCRIBING to product notifications on service {_notificationService.GetHashCode()}...");
                    _notificationService.ProductQuantityUpdated += OnProductQuantityUpdated;
                    _notificationService.ProductsChanged += OnProductsChanged;
                    _logger?.Info($"✅ PatronViewModel SUBSCRIBED to product update notifications");
                }
                else
                {
                    _logger?.Error($"❌ NOTIFICATION SERVICE NOT FOUND in PatronViewModel - Real-time updates will not work!");
                }
                
                // Subscribe to employee status change events
                if (_userSessionService != null)
                {
                    _logger?.Info($"🔗 PatronViewModel SUBSCRIBING to employee status change notifications...");
                    _userSessionService.EmployeeStatusChanged += OnEmployeeStatusChanged;
                    _logger?.Info($"✅ PatronViewModel SUBSCRIBED to employee status change notifications");
                }
                else
                {
                    _logger?.Error($"❌ USER SESSION SERVICE NOT FOUND in PatronViewModel - Employee status updates will not work!");
                }
                
                _logger?.Info("PatronViewModel initialized with event-driven employee status updates");
                
                // Load initial data
                _ = Task.Run(async () => {
                    await LoadEmployeesAsync();
                    await LoadProductsAsync();
                });
            }
            catch (Exception ex)
            {
                _logger?.Error("Error initializing PatronViewModel", ex);
                ErrorMessage = $"Error initializing: {ex.Message}";
            }
        }

        // Debounced search implementation
        private void DebounceEmployeeSearch()
        {
            // Cancel the previous timer if it exists
            _searchTimer?.Dispose();
            
            // Create a new timer that will execute the search after 300ms delay
            _searchTimer = new Timer(async _ => await PerformEmployeeSearch(), null, 300, Timeout.Infinite);
        }

        private void DebounceProductSearch()
        {
            // Cancel the previous timer if it exists
            _productSearchTimer?.Dispose();
            
            // Create a new timer that will execute the search after 300ms delay
            _productSearchTimer = new Timer(async _ => await PerformProductSearch(), null, 300, Timeout.Infinite);
        }

        private async Task PerformEmployeeSearch()
        {
            try
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    PerformEmployeeSearchSync();
                });
            }
            catch (Exception ex)
            {
                _logger?.Error("Error performing employee search", ex);
            }
        }

        private async Task PerformProductSearch()
        {
            try
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    PerformProductFilterSync();
                });
            }
            catch (Exception ex)
            {
                _logger?.Error("Error performing product search", ex);
            }
        }

        private void PerformEmployeeSearchSync()
        {
            var searchText = EmployeeSearchText?.Trim().ToLowerInvariant();
            
            FilteredEmployees.Clear();
            
            if (string.IsNullOrEmpty(searchText))
            {
                // Show all employees if search is empty
                foreach (var employee in Employees)
                {
                    FilteredEmployees.Add(employee);
                }
            }
            else
            {
                // Filter employees by name (case-insensitive)
                var filteredEmployees = Employees.Where(emp => 
                    emp.Nume?.ToLowerInvariant().Contains(searchText) == true ||
                    emp.Email?.ToLowerInvariant().Contains(searchText) == true ||
                    emp.Username?.ToLowerInvariant().Contains(searchText) == true
                ).ToList();
                
                foreach (var employee in filteredEmployees)
                {
                    FilteredEmployees.Add(employee);
                }
            }
            
            _logger?.Debug($"Employee search performed for '{searchText}', found {FilteredEmployees.Count} results");
        }

        private void PerformProductFilterSync()
        {
            var searchText = ProductSearchText?.Trim().ToLowerInvariant();
            var selectedCategory = SelectedCategory;
            
            FilteredProducts.Clear();
            
            var filteredProducts = Products.Where(product => 
            {
                // Filter by search text
                var matchesSearch = string.IsNullOrEmpty(searchText) || 
                                  product.Nume?.ToLowerInvariant().Contains(searchText) == true;
                
                // Filter by category
                var matchesCategory = selectedCategory == "All" || 
                                    product.Tip.ToString() == selectedCategory;
                
                return matchesSearch && matchesCategory;
            }).ToList();
            
            foreach (var product in filteredProducts)
            {
                FilteredProducts.Add(product);
            }
            
            _logger?.Debug($"Product filter performed for search '{searchText}' and category '{selectedCategory}', found {FilteredProducts.Count} results");
        }

        // Employee Management Methods
        public async Task LoadEmployeesAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                
                _logger?.Debug("Loading employees from database");
                var employees = await _service.AfisareAngajati();
                
                // Update UI on main thread
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Employees.Clear();
                    foreach (var employee in employees)
                    {
                        Employees.Add(employee);
                    }
                    
                    // Update filtered collection based on current search
                    PerformEmployeeSearchSync();
                });
                
                _logger?.Info($"Loaded {employees.Count} employees");
            }
            catch (Exception ex)
            {
                _logger?.Error("Error loading employees", ex);
                ErrorMessage = $"Error loading employees: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task LoadProductsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                
                _logger?.Debug("Loading products from database");
                var products = await _service.AfisareProduse();
                
                // Update UI on main thread
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Products.Clear();
                    foreach (var product in products)
                    {
                        Products.Add(product);
                    }
                    
                    // Update filtered collection based on current search and category filter
                    PerformProductFilterSync();
                });
                
                _logger?.Info($"Loaded {products.Count} products");
            }
            catch (Exception ex)
            {
                _logger?.Error("Error loading products", ex);
                ErrorMessage = $"Error loading products: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task AddEmployeeAsync()
        {
            try
            {
                _logger?.Info("Opening Add Employee dialog");
                
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    // Find the PatronWindow as parent
                    var parentWindow = GetPatronWindow();
                    
                    // Show modal overlay
                    if (parentWindow is PatronWindow patronWindow)
                    {
                        patronWindow.ShowModalOverlay();
                    }
                    
                    var dialog = new AddEmployeeDialog();
                    
                    try
                    {
                        await dialog.ShowDialog(parentWindow);
                        
                        // Check if employee was created
                        if (dialog.CreatedEmployee != null)
                        {
                            Employees.Add(dialog.CreatedEmployee);
                            _logger?.Info($"Employee {dialog.CreatedEmployee.Nume} added to UI collection");
                            
                            // Refresh filtered collection
                            await PerformEmployeeSearch();
                        }
                    }
                    finally
                    {
                        // Hide modal overlay
                        if (parentWindow is PatronWindow patronWindow2)
                        {
                            patronWindow2.HideModalOverlay();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.Error("Error opening Add Employee dialog", ex);
                ErrorMessage = $"Error opening dialog: {ex.Message}";
            }
        }

        public async Task EditEmployeeAsync(Angajat employee)
        {
            if (employee == null) return;
            
            try
            {
                _logger?.Info($"Opening Edit Employee dialog for: {employee.Nume}");
                
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    // Find the PatronWindow as parent
                    var parentWindow = GetPatronWindow();
                    
                    // Show modal overlay
                    if (parentWindow is PatronWindow patronWindow)
                    {
                        patronWindow.ShowModalOverlay();
                    }
                    
                    var dialog = new EditEmployeeDialog(employee);
                    
                    try
                    {
                        await dialog.ShowDialog(parentWindow);
                        
                        // Check if employee was updated
                        if (dialog.WasUpdated)
                        {
                            // Refresh the employee list to get updated data
                            await LoadEmployeesAsync();
                            _logger?.Info($"Employee {employee.Nume} updated, list refreshed");
                        }
                    }
                    finally
                    {
                        // Hide modal overlay
                        if (parentWindow is PatronWindow patronWindow2)
                        {
                            patronWindow2.HideModalOverlay();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error opening Edit Employee dialog for {employee.Nume}", ex);
                ErrorMessage = $"Error opening dialog: {ex.Message}";
            }
        }

        public async Task DeleteEmployeeAsync(Angajat employee)
        {
            if (employee == null) return;
            
            try
            {
                _logger?.Info($"Requesting confirmation to delete employee: {employee.Nume}");
                
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    // Find the PatronWindow as parent
                    var parentWindow = GetPatronWindow();
                    
                    // Show modal overlay
                    if (parentWindow is PatronWindow patronWindow)
                    {
                        patronWindow.ShowModalOverlay();
                    }
                    
                    var confirmDialog = new ConfirmDeleteDialog($"Are you sure you want to fire {employee.Nume}?");
                    
                    try
                    {
                        await confirmDialog.ShowDialog(parentWindow);
                        
                        if (confirmDialog.WasConfirmed)
                        {
                            _logger?.Info($"Deleting employee: {employee.Nume} (ID: {employee.Id})");
                            
                            var result = await _service.StergereAngajat(employee.Id);
                            if (result)
                            {
                                // Remove from UI collections
                                Employees.Remove(employee);
                                FilteredEmployees.Remove(employee);
                                _logger?.Info($"Successfully deleted employee: {employee.Nume}");
                            }
                            else
                            {
                                ErrorMessage = "Failed to delete employee";
                                _logger?.Warn($"Failed to delete employee: {employee.Nume}");
                            }
                        }
                        else
                        {
                            _logger?.Debug($"Employee deletion cancelled for: {employee.Nume}");
                        }
                    }
                    finally
                    {
                        // Hide modal overlay
                        if (parentWindow is PatronWindow patronWindow2)
                        {
                            patronWindow2.HideModalOverlay();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error deleting employee {employee.Nume}", ex);
                ErrorMessage = $"Error deleting employee: {ex.Message}";
            }
        }

        public async Task DeleteProductAsync(Produs product)
        {
            if (product == null) return;
            
            try
            {
                _logger?.Info($"Requesting confirmation to delete product: {product.Nume}");
                
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    // Find the PatronWindow as parent
                    var parentWindow = GetPatronWindow();
                    
                    // Show modal overlay
                    if (parentWindow is PatronWindow patronWindow)
                    {
                        patronWindow.ShowModalOverlay();
                    }
                    
                    var confirmDialog = new ConfirmDeleteDialog($"Are you sure you want to delete {product.Nume}?");
                    
                    try
                    {
                        await confirmDialog.ShowDialog(parentWindow);
                        
                        if (confirmDialog.WasConfirmed)
                        {
                            _logger?.Info($"Deleting product: {product.Nume} (ID: {product.Id})");
                            
                            var result = await _service.StergereProdus(product.Id);
                            if (result)
                            {
                                // Remove from UI collection
                                Products.Remove(product);
                                // Update filtered collection to remove the product immediately
                                PerformProductFilterSync();
                                _logger?.Info($"Successfully deleted product: {product.Nume}");
                            }
                            else
                            {
                                ErrorMessage = "Failed to delete product";
                                _logger?.Warn($"Failed to delete product: {product.Nume}");
                            }
                        }
                        else
                        {
                            _logger?.Debug($"Product deletion cancelled for: {product.Nume}");
                        }
                    }
                    finally
                    {
                        // Hide modal overlay
                        if (parentWindow is PatronWindow patronWindow2)
                        {
                            patronWindow2.HideModalOverlay();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error deleting product {product.Nume}", ex);
                ErrorMessage = $"Error deleting product: {ex.Message}";
            }
        }

        // Placeholder methods for Add/Edit operations
        private async Task AddProductAsync()
        {
            try
            {
                _logger?.Info("Opening Add Product dialog");
                
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    // Find the PatronWindow as parent
                    var parentWindow = GetPatronWindow();
                    
                    // Show modal overlay
                    if (parentWindow is PatronWindow patronWindow)
                    {
                        patronWindow.ShowModalOverlay();
                    }
                    
                    var dialog = new AddProductDialog();
                    
                    try
                    {
                        await dialog.ShowDialog(parentWindow);
                        
                        // Check if product was created
                        if (dialog.CreatedProduct != null)
                        {
                            Products.Add(dialog.CreatedProduct);
                            // Update filtered collection to show the new product immediately
                            PerformProductFilterSync();
                            _logger?.Info($"Product {dialog.CreatedProduct.Nume} added to UI collection");
                        }
                    }
                    finally
                    {
                        // Hide modal overlay
                        if (parentWindow is PatronWindow patronWindow2)
                        {
                            patronWindow2.HideModalOverlay();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.Error("Error opening Add Product dialog", ex);
                ErrorMessage = $"Error opening dialog: {ex.Message}";
            }
        }

        private async Task EditProductAsync(Produs product)
        {
            if (product == null) return;
            
            try
            {
                _logger?.Info($"Opening Edit Product dialog for: {product.Nume}");
                
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    // Find the PatronWindow as parent
                    var parentWindow = GetPatronWindow();
                    
                    // Show modal overlay
                    if (parentWindow is PatronWindow patronWindow)
                    {
                        patronWindow.ShowModalOverlay();
                    }
                    
                    var dialog = new EditProductDialog(product);
                    
                    try
                    {
                        await dialog.ShowDialog(parentWindow);
                        
                        // Check if product was updated
                        if (dialog.WasUpdated)
                        {
                            // Refresh the product list to get updated data
                            await LoadProductsAsync();
                            _logger?.Info($"Product {product.Nume} updated, list refreshed");
                        }
                    }
                    finally
                    {
                        // Hide modal overlay
                        if (parentWindow is PatronWindow patronWindow2)
                        {
                            patronWindow2.HideModalOverlay();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error opening Edit Product dialog for {product.Nume}", ex);
                ErrorMessage = $"Error opening dialog: {ex.Message}";
            }
        }

        // Refresh data method
        public async Task RefreshDataAsync()
        {
            await Task.WhenAll(LoadEmployeesAsync(), LoadProductsAsync());
        }

        // Event handler for employee status changes
        private async void OnEmployeeStatusChanged(object sender, EmployeeStatusChangedEventArgs e)
        {
            try
            {
                _logger?.Info($"🔔 PatronViewModel EMPLOYEE STATUS NOTIFICATION: {e.EmployeeName} is now {(e.IsOnline ? "ONLINE" : "OFFLINE")}");
                
                // Immediately refresh the employee status display
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _logger?.Debug("Employee status changed - forcing UI refresh");
                    
                    // Create a temporary collection to force UI refresh
                    var currentEmployees = FilteredEmployees.ToList();
                    FilteredEmployees.Clear();
                    
                    // Add employees back one by one to trigger converter re-evaluation
                    foreach (var employee in currentEmployees)
                    {
                        FilteredEmployees.Add(employee);
                    }
                    
                    _logger?.Debug($"Forced UI refresh for {currentEmployees.Count} employees due to {e.EmployeeName} status change");
                });
                
                _logger?.Info($"✅ PatronViewModel updated employee status display for {e.EmployeeName}");
            }
            catch (Exception ex)
            {
                _logger?.Error($"❌ PatronViewModel error handling employee status change notification for {e.EmployeeName}", ex);
            }
        }

        // Cleanup resources
        public void Dispose()
        {
            // Unsubscribe from events to prevent memory leaks
            if (_notificationService != null)
            {
                _notificationService.ProductQuantityUpdated -= OnProductQuantityUpdated;
                _notificationService.ProductsChanged -= OnProductsChanged;
                _logger?.Debug("PatronViewModel unsubscribed from product notifications");
            }
            
            if (_userSessionService != null)
            {
                _userSessionService.EmployeeStatusChanged -= OnEmployeeStatusChanged;
                _logger?.Debug("PatronViewModel unsubscribed from employee status change notifications");
            }
            
            _searchTimer?.Dispose();
            _productSearchTimer?.Dispose();
            _logger?.Debug("PatronViewModel disposed");
        }

        // Helper method to find the PatronWindow
        private Avalonia.Controls.Window? GetPatronWindow()
        {
            try
            {
                if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                {
                    // Look for PatronWindow in the active windows
                    foreach (var window in desktop.Windows)
                    {
                        if (window is PatronWindow)
                        {
                            return window;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger?.Error("Error finding PatronWindow", ex);
                return null;
            }
        }

        // Notification Event Handlers
        private async void OnProductQuantityUpdated(object sender, ProductQuantityUpdatedEventArgs e)
        {
            try
            {
                _logger?.Info($"🔔 PatronViewModel NOTIFICATION RECEIVED: Product {e.ProductId} quantity updated to {e.NewQuantity}");
                
                // Update the product in the collections on the UI thread
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    // Find the product in the main collection
                    var product = Products.FirstOrDefault(p => p.Id == e.ProductId);
                    if (product != null)
                    {
                        product.Cantitate = e.NewQuantity;
                        _logger?.Info($"✅ PatronViewModel updated product {product.Nume} quantity to {e.NewQuantity}");
                    }
                    else
                    {
                        _logger?.Warn($"⚠️ PatronViewModel: Product {e.ProductId} not found in Products collection");
                    }
                    
                    // Refresh the filtered collection to reflect changes
                    PerformProductFilterSync();
                });
            }
            catch (Exception ex)
            {
                _logger?.Error("❌ PatronViewModel error handling product quantity update notification", ex);
            }
        }

        private async void OnProductsChanged(object sender, EventArgs e)
        {
            try
            {
                _logger?.Info("🔔 PatronViewModel NOTIFICATION RECEIVED: Products changed - reloading product list");
                
                // Reload the entire product list when products are added/removed
                await LoadProductsAsync();
            }
            catch (Exception ex)
            {
                _logger?.Error("❌ PatronViewModel error handling products changed notification", ex);
            }
        }
    }
} 