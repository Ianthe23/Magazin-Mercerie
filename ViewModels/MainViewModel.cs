using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using magazin_mercerie.Service;
using System.Collections.Generic;
using Avalonia.Controls;
using magazin_mercerie.Models;

namespace magazin_mercerie.ViewModels
{
    // Category display model
    public class CategoryDisplayModel
    {
        public string Name { get; set; }
        public string IconData { get; set; }
        public TipProdus? ProductType { get; set; } // null for "All Products"
        public bool IsSelected { get; set; }
        
        public CategoryDisplayModel(string name, string iconData, TipProdus? productType = null)
        {
            Name = name;
            IconData = iconData;
            ProductType = productType;
        }
    }

    // Wrapper class for display purposes
    public class ProductDisplayModel
    {
        public Produs Product { get; set; }
        public string Nume => Product?.Nume ?? "";
        public TipProdus Tip => Product?.Tip ?? TipProdus.Ghem;
        public string FormattedPrice => Product?.Pret != null ? $"${Product.Pret:F2}" : "$0.00";
        public string FormattedStock => Product?.Cantitate != null ? $"{Product.Cantitate:F0}" : "0";
        public decimal Pret => Product?.Pret ?? 0;
        public decimal Cantitate => Product?.Cantitate ?? 0;

        public ProductDisplayModel(Produs product)
        {
            Product = product;
        }
    }

    public class MainViewModel : ViewModelBase
    {
        private readonly ILog _logger;
        private readonly IService _service;
        private readonly ICartService _cartService;
        private readonly IUserSessionService _userSessionService;
        private readonly IProductUpdateNotificationService _notificationService;
        private Timer _searchTimer;
        
        // Observable collections for data binding
        public ObservableCollection<Produs> Products { get; set; }
        public ObservableCollection<Produs> FilteredProducts { get; set; }
        public ObservableCollection<CategoryDisplayModel> Categories { get; set; }
        public ObservableCollection<Comanda> UserOrders { get; set; }
        
        // Search and filter properties
        private string _searchText = string.Empty;
        private string _selectedCategory = "All Products";
        private bool _isLoading = false;
        private string _errorMessage = string.Empty;
        private int _productCount = 0;
        private int _cartItemCount = 0;
        
        // View mode properties
        private bool _isProductsView = true;
        private bool _isOrdersView = false;
        
        // User properties
        private string _currentUserName = "Guest";
        private string _currentUserInitial = "G";

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                DebounceSearch();
            }
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                FilterProducts();
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

        public int ProductCount
        {
            get => _productCount;
            set
            {
                _productCount = value;
                OnPropertyChanged();
            }
        }

        public int CartItemCount
        {
            get => _cartItemCount;
            set
            {
                _cartItemCount = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsProductsView
        {
            get => _isProductsView;
            set
            {
                _isProductsView = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsOrdersView
        {
            get => _isOrdersView;
            set
            {
                _isOrdersView = value;
                OnPropertyChanged();
            }
        }
        
        public string CurrentUserName
        {
            get => _currentUserName;
            set
            {
                _currentUserName = value;
                OnPropertyChanged();
            }
        }
        
        public string CurrentUserInitial
        {
            get => _currentUserInitial;
            set
            {
                _currentUserInitial = value;
                OnPropertyChanged();
            }
        }

        // Cart properties
        public ICartService CartService => _cartService;

        // Commands
        public ICommand LoadProductsCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand SelectCategoryCommand { get; }
        public ICommand ShowProductsCommand { get; private set; }
        public ICommand ShowOrdersCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }
        public ICommand ViewOrderDetailsCommand { get; private set; }

        public MainViewModel()
        {
            try
            {
                _logger = LogManager.GetLogger(typeof(MainViewModel));
                _logger?.Info($"🏗️ CREATING MainViewModel - checking services...");
                
                _service = App.ServiceProvider?.GetService<IService>();
                _logger?.Info($"Service: {(_service != null ? "✅ Found" : "❌ Missing")}");
                
                _cartService = App.ServiceProvider?.GetService<ICartService>();
                _logger?.Info($"CartService: {(_cartService != null ? "✅ Found" : "❌ Missing")}");
                
                _userSessionService = App.ServiceProvider?.GetService<IUserSessionService>();
                _logger?.Info($"UserSessionService: {(_userSessionService != null ? "✅ Found" : "❌ Missing")}");
                
                _notificationService = App.ServiceProvider?.GetService<IProductUpdateNotificationService>();
                _logger?.Info($"NotificationService: {(_notificationService != null ? "✅ Found" : "❌ Missing")}");
                
                if (App.ServiceProvider == null)
                {
                    _logger?.Error($"❌ CRITICAL: App.ServiceProvider is NULL!");
                }
                
                // DEBUG: Log service instance hashes for identity tracking
                _logger?.Info($"🔍 SERVICE INSTANCES:");
                _logger?.Info($"   NotificationService HashCode: {_notificationService?.GetHashCode()}");
                _logger?.Info($"   MainViewModel HashCode: {this.GetHashCode()}");
                
                // Initialize collections
                Products = new ObservableCollection<Produs>();
                FilteredProducts = new ObservableCollection<Produs>();
                Categories = new ObservableCollection<CategoryDisplayModel>();
                UserOrders = new ObservableCollection<Comanda>();
                
                // Initialize commands
                LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync);
                AddToCartCommand = new RelayCommand<Produs>(AddToCart);
                SelectCategoryCommand = new RelayCommand<CategoryDisplayModel>(SelectCategory);
                ShowProductsCommand = new RelayCommand(ShowProducts);
                ShowOrdersCommand = new RelayCommand(ShowOrders);
                LogoutCommand = new AsyncRelayCommand(LogoutAsync);
                ViewOrderDetailsCommand = new RelayCommand<Comanda>(ViewOrderDetails);
                
                // Subscribe to cart changes
                if (_cartService != null)
                {
                    _cartService.CartChanged += OnCartChanged;
                }
                
                // Subscribe to product update notifications
                if (_notificationService != null)
                {
                    _logger?.Info($"🔗 MainViewModel SUBSCRIBING to notifications on service {_notificationService.GetHashCode()}...");
                    _notificationService.ProductQuantityUpdated += OnProductQuantityUpdated;
                    _notificationService.ProductsChanged += OnProductsChanged;
                    _notificationService.OrderStatusUpdated += OnOrderStatusUpdated;
                    _logger?.Info($"✅ MainViewModel SUBSCRIBED to all notifications");
                    
                    // DEBUG: Verify subscription worked by checking invocation list
                    var quantityEvent = typeof(IProductUpdateNotificationService).GetEvent("ProductQuantityUpdated");
                    if (quantityEvent != null)
                    {
                        var field = _notificationService.GetType().GetField("ProductQuantityUpdated", 
                            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        if (field?.GetValue(_notificationService) is Delegate del)
                        {
                            var subscribers = del.GetInvocationList().Length;
                            _logger?.Info($"🔍 VERIFICATION: ProductQuantityUpdated has {subscribers} subscribers");
                        }
                        else
                        {
                            _logger?.Warn($"⚠️ VERIFICATION FAILED: Could not verify subscription");
                        }
                    }
                }
                else
                {
                    _logger?.Error($"❌ NOTIFICATION SERVICE NOT FOUND in MainViewModel - Real-time updates will not work!");
                }
                
                // Initialize user data
                InitializeUserData();
                
                // Initialize categories
                InitializeCategories();
                
                _logger?.Info("MainViewModel initialized");
                
                // Load initial data
                _ = Task.Run(async () => await LoadProductsAsync());
            }
            catch (Exception ex)
            {
                _logger?.Error("Error initializing MainViewModel", ex);
                ErrorMessage = $"Error initializing: {ex.Message}";
            }
        }
        
        private void InitializeUserData()
        {
            var currentUser = _userSessionService?.CurrentUser;
            if (currentUser != null)
            {
                CurrentUserName = currentUser.Nume;
                CurrentUserInitial = !string.IsNullOrEmpty(currentUser.Nume) ? 
                    currentUser.Nume.Substring(0, 1).ToUpper() : "U";
            }
        }
        
        private void InitializeCategories()
        {
            Categories.Clear();
            
            // Add "All Products" option
            Categories.Add(new CategoryDisplayModel("All Products", "{StaticResource SemiIconList}", null) { IsSelected = true });
            
            // Add categories based on TipProdus enum
            Categories.Add(new CategoryDisplayModel("Ghem", "{StaticResource SemiIconThread}", TipProdus.Ghem));
            Categories.Add(new CategoryDisplayModel("Croseta", "{StaticResource SemiIconTools}", TipProdus.Croseta));
            Categories.Add(new CategoryDisplayModel("Alte Produse", "{StaticResource SemiIconStar}", TipProdus.AlteProduse));
        }

        public async Task LoadProductsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                
                _logger?.Debug("Loading products from database");
                var products = await _service.AfisareProduse();
                
                // Debug: Log each product's data
                foreach (var product in products)
                {
                    _logger?.Debug($"Loaded Product: Name='{product.Nume}', Price={product.Pret}, Stock={product.Cantitate}, Type={product.Tip}");
                }
                
                // Update UI on main thread
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Products.Clear();
                    foreach (var product in products)
                    {
                        Products.Add(product);
                    }
                    
                    // Update filtered collection and count
                    FilterProducts();
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

        private void FilterProducts()
        {
            var searchText = SearchText?.Trim().ToLowerInvariant();
            var selectedCategory = SelectedCategory;
            
            _logger?.Debug($"FilterProducts called - SearchText: '{searchText}', Category: '{selectedCategory}'");
            _logger?.Debug($"Products collection has {Products.Count} items");
            
            FilteredProducts.Clear();
            
            var filteredProducts = Products.Where(product => 
            {
                // Filter by search text
                var matchesSearch = string.IsNullOrEmpty(searchText) || 
                                  product.Nume?.ToLowerInvariant().Contains(searchText) == true;
                
                // Filter by category
                var matchesCategory = selectedCategory == "All Products" || 
                                    product.Tip.ToString() == selectedCategory;
                
                var result = matchesSearch && matchesCategory;
                _logger?.Debug($"Product '{product.Nume}': matchesSearch={matchesSearch}, matchesCategory={matchesCategory}, result={result}");
                return result;
            }).ToList();
            
            _logger?.Debug($"Filtered {filteredProducts.Count} products from {Products.Count} total");
            
            foreach (var product in filteredProducts)
            {
                FilteredProducts.Add(product);
                _logger?.Debug($"Added to FilteredProducts: '{product.Nume}' - Price={product.Pret}, Stock={product.Cantitate}");
            }
            
            ProductCount = FilteredProducts.Count;
            
            _logger?.Debug($"FilteredProducts final count: {FilteredProducts.Count}");
        }

        private void DebounceSearch()
        {
            // Cancel the previous timer if it exists
            _searchTimer?.Dispose();
            
            // Create a new timer that will execute the search after 300ms delay
            _searchTimer = new Timer(async _ => await PerformSearch(), null, 300, Timeout.Infinite);
        }

        private async Task PerformSearch()
        {
            try
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    FilterProducts();
                });
            }
            catch (Exception ex)
            {
                _logger?.Error("Error performing search", ex);
            }
        }

        public void AddToCart(Produs product)
        {
            try
            {
                if (product == null)
                {
                    _logger?.Warn("Attempted to add null product to cart");
                    return;
                }
                
                _cartService?.AddToCart(product);
                _logger?.Info($"Added product '{product.Nume}' to cart");
            }
            catch (Exception ex)
            {
                _logger?.Error("Error adding product to cart", ex);
                ErrorMessage = $"Error adding product to cart: {ex.Message}";
            }
        }

        private void OnCartChanged(object sender, EventArgs e)
        {
            // Update cart item count when cart changes
            CartItemCount = _cartService?.TotalItemCount ?? 0;
        }

        private async void OnProductQuantityUpdated(object sender, ProductQuantityUpdatedEventArgs e)
        {
            try
            {
                _logger?.Info($"🔔 NOTIFICATION RECEIVED: Product {e.ProductId} quantity updated to {e.NewQuantity}");
                
                // Update the product in the collections on the UI thread
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    // Find and update the product in the Products collection
                    var product = Products.FirstOrDefault(p => p.Id == e.ProductId);
                    if (product != null)
                    {
                        var oldQuantity = product.Cantitate;
                        product.Cantitate = e.NewQuantity;
                        _logger?.Info($"✅ UPDATED PRODUCT in Products collection: '{product.Nume}' quantity from {oldQuantity} to {e.NewQuantity}");
                        
                        // CRITICAL FIX: Force UI refresh by recreating FilteredProducts collection
                        // This ensures the UI sees the changes to product properties
                        _logger?.Info($"🔄 FORCING UI REFRESH: Recreating FilteredProducts collection...");
                        FilterProducts(); // This will recreate the FilteredProducts collection with updated values
                        
                        _logger?.Info($"✅ FilteredProducts collection refreshed - UI should now show updated stock: {e.NewQuantity}");
                    }
                    else
                    {
                        _logger?.Warn($"⚠️ PRODUCT NOT FOUND: Could not find product {e.ProductId} in Products collection");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.Error("❌ Error handling product quantity update notification", ex);
            }
        }

        private async void OnProductsChanged(object sender, EventArgs e)
        {
            try
            {
                _logger?.Debug("Received products changed notification - reloading product list");
                
                // Reload the entire product list when products are added/removed
                await LoadProductsAsync();
            }
            catch (Exception ex)
            {
                _logger?.Error("Error handling products changed notification", ex);
            }
        }

        private async void OnOrderStatusUpdated(object sender, OrderStatusUpdatedEventArgs e)
        {
            try
            {
                _logger?.Info($"🔔 NOTIFICATION RECEIVED: Order {e.OrderId} status updated to {e.NewStatus}");
                
                // Check if this notification is for the current client
                var currentClient = _userSessionService?.GetCurrentClient();
                if (currentClient == null || currentClient.Id != e.ClientId)
                {
                    _logger?.Info($"🔕 NOTIFICATION IGNORED: Not for current client {currentClient?.Username ?? "none"} (notification for client {e.ClientId})");
                    return;
                }
                
                // Update the order in the collections on the UI thread
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    // Find and update the order in the UserOrders collection
                    var order = UserOrders.FirstOrDefault(o => o.Id == e.OrderId);
                    if (order != null)
                    {
                        var oldStatus = order.Status;
                        order.Status = e.NewStatus;
                        _logger?.Info($"✅ UPDATED ORDER in UserOrders collection: '{order.Id}' status from {oldStatus} to {e.NewStatus}");
                        
                        // CRITICAL FIX: Force UI refresh by reloading user orders
                        // This ensures the orders view shows the updated status immediately
                        _logger?.Info($"🔄 FORCING UI REFRESH: Reloading user orders to reflect status change...");
                        await LoadUserOrdersAsync();
                        _logger?.Info($"✅ User orders reloaded - Orders view should now show updated status: {e.NewStatus}");
                    }
                    else
                    {
                        _logger?.Warn($"⚠️ ORDER NOT FOUND: Could not find order {e.OrderId} in UserOrders collection");
                        _logger?.Info($"🔄 LOADING USER ORDERS: Refreshing order list to include updated order...");
                        
                        // Load user orders to ensure we have the latest data
                        await LoadUserOrdersAsync();
                        
                        // Try to find the order again after loading
                        order = UserOrders.FirstOrDefault(o => o.Id == e.OrderId);
                        if (order != null)
                        {
                            order.Status = e.NewStatus;
                            _logger?.Info($"✅ FOUND AND UPDATED ORDER after reload: '{order.Id}' status to {e.NewStatus}");
                        }
                        else
                        {
                            _logger?.Warn($"⚠️ ORDER STILL NOT FOUND after reload: {e.OrderId}");
                        }
                    }
                    
                    // Show notification to client regardless of whether order was found in collection
                    _logger?.Info($"📢 SHOWING NOTIFICATION to client: Order {e.OrderId} status changed to {e.NewStatus}");
                    _ = Task.Run(async () => await ShowOrderStatusNotificationAsync(e.OrderId, e.NewStatus));
                });
            }
            catch (Exception ex)
            {
                _logger?.Error("❌ Error handling order status update notification", ex);
            }
        }

        private void SelectCategory(CategoryDisplayModel category)
        {
            if (category == null) return;
            
            // Update selection state
            foreach (var cat in Categories)
            {
                cat.IsSelected = cat == category;
            }
            
            SelectedCategory = category.Name;
            OnPropertyChanged(nameof(Categories));
        }
        
        private void ShowProducts()
        {
            IsProductsView = true;
            IsOrdersView = false;
        }
        
        private async void ShowOrders()
        {
            IsProductsView = false;
            IsOrdersView = true;
            await LoadUserOrdersAsync();
        }
        
        private async Task LoadUserOrdersAsync()
        {
            try
            {
                var currentClient = _userSessionService?.GetCurrentClient();
                if (currentClient == null) return;
                
                _logger?.Debug($"Loading orders for client: {currentClient.Username}");
                var orders = await _service.AfisareComenziClient(currentClient.Id);
                
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    UserOrders.Clear();
                    foreach (var order in orders)
                    {
                        UserOrders.Add(order);
                    }
                });
                
                _logger?.Info($"Loaded {orders.Count} orders for user");
            }
            catch (Exception ex)
            {
                _logger?.Error("Error loading user orders", ex);
                ErrorMessage = $"Error loading orders: {ex.Message}";
            }
        }
        
        private async Task LogoutAsync()
        {
            try
            {
                // Check if cart has items
                if (_cartService?.HasItems == true)
                {
                    var result = await ShowLogoutConfirmationAsync();
                    if (!result) return; // User cancelled
                }
                
                // Clear user session
                _userSessionService?.ClearCurrentUser();
                
                // Clear cart
                _cartService?.ClearCart();
                
                // Navigate back to login
                await NavigateToLoginAsync();
            }
            catch (Exception ex)
            {
                _logger?.Error("Error during logout", ex);
            }
        }
        
        private async Task<bool> ShowLogoutConfirmationAsync()
        {
            var confirmDialog = new Window()
            {
                Title = "Confirm Logout",
                Width = 400,
                Height = 250,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Topmost = true
            };
            
            var stackPanel = new StackPanel()
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 20
            };
            
            stackPanel.Children.Add(new TextBlock()
            {
                Text = "You have items in your cart. Are you sure you want to logout?\n\nYour cart will be cleared.",
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                FontSize = 14,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                TextAlignment = Avalonia.Media.TextAlignment.Center
            });
            
            var buttonPanel = new StackPanel()
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Spacing = 15
            };
            
            var logoutButton = new Button()
            {
                Content = "Logout",
                Background = Avalonia.Media.Brushes.Red,
                Foreground = Avalonia.Media.Brushes.White,
                Padding = new Avalonia.Thickness(20, 10),
                MinWidth = 120
            };
            
            var cancelButton = new Button()
            {
                Content = "Cancel",
                Background = Avalonia.Media.Brushes.Gray,
                Foreground = Avalonia.Media.Brushes.White,
                Padding = new Avalonia.Thickness(20, 10),
                MinWidth = 120
            };
            
            bool? result = null;
            
            logoutButton.Click += (s, e) => {
                result = true;
                confirmDialog.Close();
            };
            
            cancelButton.Click += (s, e) => {
                result = false;
                confirmDialog.Close();
            };
            
            buttonPanel.Children.Add(logoutButton);
            buttonPanel.Children.Add(cancelButton);
            
            stackPanel.Children.Add(buttonPanel);
            confirmDialog.Content = stackPanel;
            
            confirmDialog.Show();
            await WaitForWindowToClose(confirmDialog);
            
            return result == true;
        }
        
        private async Task WaitForWindowToClose(Window window)
        {
            var tcs = new TaskCompletionSource<bool>();
            
            void OnClosed(object sender, EventArgs e)
            {
                window.Closed -= OnClosed;
                tcs.SetResult(true);
            }
            
            window.Closed += OnClosed;
            await tcs.Task;
        }
        
        private async Task NavigateToLoginAsync()
        {
            try
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var loginWindow = new Views.LoginViews.LoginWindow
                    {
                        DataContext = new LoginViewModel()
                    };
                    
                    // Show the login window first
                    loginWindow.Show();
                    
                    // Set login window as MainWindow BEFORE closing the current window
                    if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        var currentMainWindow = desktop.MainWindow;
                        desktop.MainWindow = loginWindow;
                        
                        // Now close the previous window
                        currentMainWindow?.Close();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.Error("Error navigating to login", ex);
            }
        }

        private void ViewOrderDetails(Comanda order)
        {
            if (order == null) return;
            
            try
            {
                var orderDetailsWindow = new Views.ClientViews.OrderDetailsWindow(order);
                orderDetailsWindow.Show();
                _logger?.Info($"View details requested for order: {order.Id}");
            }
            catch (Exception ex)
            {
                _logger?.Error("Error opening order details", ex);
            }
        }

        private async Task ShowOrderStatusNotificationAsync(Guid orderId, Status newStatus)
        {
            try
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    // Find the parent window (MainWindow)
                    Window parentWindow = null;
                    if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        parentWindow = desktop.Windows.FirstOrDefault(w => w.Title == "Magazin Mercerie" || w.GetType().Name.Contains("MainWindow"));
                    }

                    var notificationWindow = new Window()
                    {
                        Title = "Order Status Update",
                        Width = 450,
                        Height = 250,
                        Topmost = true,
                        CanResize = false,
                        ShowInTaskbar = false,
                        SystemDecorations = Avalonia.Controls.SystemDecorations.BorderOnly
                    };

                    // Position relative to parent window
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

                    // Status icon and color based on status
                    var statusIcon = newStatus switch
                    {
                        Status.Preluat => "📦",
                        Status.Procesat => "⚙️",
                        Status.Finalizat => "✅",
                        _ => "📋"
                    };

                    var statusColor = newStatus switch
                    {
                        Status.Preluat => Avalonia.Media.Brushes.Orange,
                        Status.Procesat => Avalonia.Media.Brushes.DodgerBlue, // Changed from Blue to DodgerBlue
                        Status.Finalizat => Avalonia.Media.Brushes.Green,
                        _ => Avalonia.Media.Brushes.Gray
                    };

                    stackPanel.Children.Add(new TextBlock()
                    {
                        Text = $"{statusIcon} Order Status Update",
                        FontSize = 18,
                        FontWeight = Avalonia.Media.FontWeight.Bold,
                        Foreground = statusColor,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                    });

                    stackPanel.Children.Add(new TextBlock()
                    {
                        Text = $"Your order has been updated to:",
                        FontSize = 14,
                        Foreground = Avalonia.Media.Brushes.White, // White text on dark background
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                    });

                    stackPanel.Children.Add(new TextBlock()
                    {
                        Text = $"{newStatus}",
                        FontSize = 16,
                        FontWeight = Avalonia.Media.FontWeight.Bold,
                        Foreground = statusColor,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                    });

                    var okButton = new Button()
                    {
                        Content = "OK",
                        Background = statusColor,
                        Foreground = Avalonia.Media.Brushes.White,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        Padding = new Avalonia.Thickness(30, 8),
                        Margin = new Avalonia.Thickness(0, 10, 0, 0),
                        FontWeight = Avalonia.Media.FontWeight.Bold
                    };

                    okButton.Click += (s, e) => notificationWindow.Close();
                    stackPanel.Children.Add(okButton);
                    
                    // Set dark background for the entire window to match app theme
                    notificationWindow.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(35, 36, 42)); // #23242a
                    notificationWindow.Content = stackPanel;

                    notificationWindow.Show();

                    // Auto-close after 5 seconds
                    await Task.Delay(5000);
                    if (notificationWindow.IsVisible)
                    {
                        notificationWindow.Close();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.Error("Error showing order status notification", ex);
            }
        }

        // Cleanup resources
        public void Dispose()
        {
            _searchTimer?.Dispose();
            
            // Unsubscribe from events
            if (_cartService != null)
            {
                _cartService.CartChanged -= OnCartChanged;
            }
            
            if (_notificationService != null)
            {
                _notificationService.ProductQuantityUpdated -= OnProductQuantityUpdated;
                _notificationService.ProductsChanged -= OnProductsChanged;
                _notificationService.OrderStatusUpdated -= OnOrderStatusUpdated;
            }
            
            _logger?.Debug("MainViewModel disposed");
        }
    }
} 