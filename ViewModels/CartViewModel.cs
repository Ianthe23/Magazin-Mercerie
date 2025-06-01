using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using magazin_mercerie.Service;
using magazin_mercerie.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Media;
using Avalonia.Layout;

namespace magazin_mercerie.ViewModels
{
    public class CartViewModel : ViewModelBase
    {
        private readonly ILog _logger;
        private readonly ICartService _cartService;
        private readonly IService _service;
        private readonly IUserSessionService _userSessionService;

        public ObservableCollection<CartItem> CartItems => _cartService?.CartItems ?? new ObservableCollection<CartItem>();
        
        public bool HasItems => _cartService?.HasItems ?? false;
        
        public decimal TotalPrice => _cartService?.TotalPrice ?? 0;
        
        public int TotalItemCount => _cartService?.TotalItemCount ?? 0;

        // Commands
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand PlaceOrderCommand { get; }

        public CartViewModel()
        {
            _logger = LogManager.GetLogger(typeof(CartViewModel));
            _cartService = App.ServiceProvider?.GetService<ICartService>();
            _service = App.ServiceProvider?.GetService<IService>();
            _userSessionService = App.ServiceProvider?.GetService<IUserSessionService>();
            
            // Initialize commands
            IncreaseQuantityCommand = new RelayCommand<CartItem>(IncreaseQuantity);
            DecreaseQuantityCommand = new RelayCommand<CartItem>(DecreaseQuantity);
            RemoveFromCartCommand = new RelayCommand<CartItem>(RemoveFromCart);
            ClearCartCommand = new RelayCommand(ClearCart);
            PlaceOrderCommand = new AsyncRelayCommand(PlaceOrderAsync);
            
            // Subscribe to cart changes to update UI
            if (_cartService != null)
            {
                _cartService.CartChanged += OnCartChanged;
            }
            
            _logger?.Info("CartViewModel initialized");
        }

        private void IncreaseQuantity(CartItem cartItem)
        {
            if (cartItem != null)
            {
                _logger?.Debug($"Increasing quantity for {cartItem.Product.Nume} from {cartItem.Quantity} to {cartItem.Quantity + 1}");
                _cartService?.UpdateQuantity(cartItem.Product, cartItem.Quantity + 1);
            }
        }

        private void DecreaseQuantity(CartItem cartItem)
        {
            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    _logger?.Debug($"Decreasing quantity for {cartItem.Product.Nume} from {cartItem.Quantity} to {cartItem.Quantity - 1}");
                    _cartService?.UpdateQuantity(cartItem.Product, cartItem.Quantity - 1);
                }
                else
                {
                    _logger?.Debug($"Removing {cartItem.Product.Nume} from cart (quantity was 1)");
                    RemoveFromCart(cartItem);
                }
            }
        }

        private void RemoveFromCart(CartItem cartItem)
        {
            if (cartItem != null)
            {
                _cartService?.RemoveFromCart(cartItem.Product);
            }
        }

        private void ClearCart()
        {
            _cartService?.ClearCart();
        }

        private async Task PlaceOrderAsync()
        {
            try
            {
                if (!HasItems)
                {
                    _logger?.Warn("Attempted to place order with empty cart");
                    return;
                }

                var currentClient = _userSessionService?.GetCurrentClient();
                _logger?.Debug($"CartViewModel using UserSessionService instance: {_userSessionService?.GetHashCode()}");
                _logger?.Debug($"Current client from session service: {(currentClient?.Username ?? "null")}");
                
                if (currentClient == null)
                {
                    _logger?.Error("No current client found for placing order");
                    await ShowErrorNotificationAsync("Error", "Please log in to place an order.");
                    return;
                }

                _logger?.Info($"Placing order for client {currentClient.Username} with {TotalItemCount} items, total: {TotalPrice:F2} lei");
                
                // Store values before clearing cart
                var orderTotal = TotalPrice;
                var itemCount = TotalItemCount;
                
                // Prepare product quantities for the order
                var productQuantities = new Dictionary<Guid, decimal>();
                
                foreach (var cartItem in CartItems)
                {
                    productQuantities[cartItem.Product.Id] = cartItem.Quantity;
                }

                // Get the employee with the least orders for automatic load balancing
                _logger?.Debug("🔄 Using automatic load balancing to assign employee...");
                var assignedEmployee = await _service.GetEmployeeWithLeastOrders();
                
                if (assignedEmployee == null)
                {
                    _logger?.Error("No employees available to assign to the order");
                    await ShowErrorNotificationAsync("Error", "No employees available to process your order. Please try again later.");
                    return;
                }

                // Create the order with actual quantities
                var order = await _service.PlasareComandaWithQuantities(
                    currentClient.Id, 
                    assignedEmployee.Id, 
                    productQuantities, 
                    Status.Preluat // Start with "Preluat" status
                );

                if (order != null)
                {
                    _logger?.Info($"Order placed successfully with ID: {order.Id}");
                    
                    // Show success notification BEFORE clearing cart
                    await ShowSuccessNotificationAsync("Order Placed Successfully!", 
                        $"Your order has been placed successfully!\n\n" +
                        $"Order ID: {order.Id}\n" +
                        $"Items: {itemCount}\n" +
                        $"Total: {orderTotal:F2} lei\n\n" +
                        $"Auto-Assigned Employee: {assignedEmployee.Nume}\n" +
                        $"Status: {order.Status}\n\n" +
                        $"✨ Employee selected automatically for optimal load balancing");
                    
                    // Clear the cart after successful order placement
                    ClearCart();
                    
                    _logger?.Info($"Order placed successfully! Order ID: {order.Id}, Total: {orderTotal:F2} lei, Items: {itemCount}");
                }
                else
                {
                    _logger?.Error("Failed to create order");
                    await ShowErrorNotificationAsync("Order Failed", "Failed to place your order. Please try again.");
                }
            }
            catch (Exception ex)
            {
                _logger?.Error("Error placing order", ex);
                await ShowErrorNotificationAsync("Error", $"An error occurred while placing your order: {ex.Message}");
            }
        }

        private void OnCartChanged(object sender, EventArgs e)
        {
            // Notify UI about property changes
            OnPropertyChanged(nameof(HasItems));
            OnPropertyChanged(nameof(TotalPrice));
            OnPropertyChanged(nameof(TotalItemCount));
            OnPropertyChanged(nameof(CartItems));
        }

        private async Task ShowSuccessNotificationAsync(string title, string message)
        {
            try
            {
                // Find the parent window (MainWindow)
                Window parentWindow = null;
                if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                {
                    parentWindow = desktop.Windows.FirstOrDefault(w => w.Title == "Magazin Mercerie" || w.GetType().Name.Contains("MainWindow"));
                }

                var messageBox = new Window
                {
                    Title = title,
                    Width = 450,
                    Height = 300,
                    Topmost = true,
                    CanResize = false,
                    ShowInTaskbar = false,
                    SystemDecorations = Avalonia.Controls.SystemDecorations.BorderOnly,
                    Background = new SolidColorBrush(Color.FromRgb(35, 36, 42)) // #23242a - dark theme background
                };

                // Position relative to parent window
                if (parentWindow != null)
                {
                    messageBox.WindowStartupLocation = WindowStartupLocation.Manual;
                    var parentBounds = parentWindow.Bounds;
                    messageBox.Position = new Avalonia.PixelPoint(
                        (int)(parentBounds.X + (parentBounds.Width - 450) / 2),
                        (int)(parentBounds.Y + 100) // Offset from top of parent
                    );
                }
                else
                {
                    messageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }

                var stackPanel = new StackPanel
                {
                    Margin = new Thickness(20),
                    Background = new SolidColorBrush(Color.FromRgb(35, 36, 42)), // #23242a - dark theme background
                    Children =
                    {
                        new TextBlock()
                        {
                            Text = "✅ Order Placed Successfully!",
                            FontSize = 18,
                            FontWeight = FontWeight.Bold,
                            Foreground = Brushes.Green,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Margin = new Thickness(0, 0, 0, 15)
                        },
                        new TextBlock 
                        { 
                            Text = message,
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(0, 0, 0, 20),
                            FontSize = 14,
                            Foreground = Brushes.White, // White text on dark background
                            HorizontalAlignment = HorizontalAlignment.Center,
                            TextAlignment = TextAlignment.Center
                        },
                        new Button 
                        { 
                            Content = "OK",
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Width = 100,
                            Height = 35,
                            Background = Brushes.Green,
                            Foreground = Brushes.White,
                            FontSize = 14,
                            FontWeight = FontWeight.Bold,
                            Margin = new Thickness(0, 10, 0, 0)
                        }
                    }
                };

                messageBox.Content = stackPanel;
                
                // Handle the button click to close the message box
                if (stackPanel.Children.Count > 2 && stackPanel.Children[2] is Button button)
                {
                    button.Click += (sender, e) => messageBox.Close();
                }
                
                // Show the window
                messageBox.Show();
                
                // Auto-close after 7 seconds (longer than status notification since it has more info)
                _ = Task.Run(async () =>
                {
                    await Task.Delay(7000);
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (messageBox.IsVisible)
                        {
                            messageBox.Close();
                        }
                    });
                });
                
                // Wait for the window to be closed
                await WaitForWindowToClose(messageBox);
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error showing success notification: {ex.Message}", ex);
            }
        }

        private async Task ShowErrorNotificationAsync(string title, string message)
        {
            try
            {
                // Find the parent window (MainWindow)
                Window parentWindow = null;
                if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                {
                    parentWindow = desktop.Windows.FirstOrDefault(w => w.Title == "Magazin Mercerie" || w.GetType().Name.Contains("MainWindow"));
                }

                var messageBox = new Window
                {
                    Title = title,
                    Width = 450,
                    Height = 250,
                    Topmost = true,
                    CanResize = false,
                    ShowInTaskbar = false,
                    SystemDecorations = Avalonia.Controls.SystemDecorations.BorderOnly,
                    Background = new SolidColorBrush(Color.FromRgb(35, 36, 42)) // #23242a - dark theme background
                };

                // Position relative to parent window
                if (parentWindow != null)
                {
                    messageBox.WindowStartupLocation = WindowStartupLocation.Manual;
                    var parentBounds = parentWindow.Bounds;
                    messageBox.Position = new Avalonia.PixelPoint(
                        (int)(parentBounds.X + (parentBounds.Width - 450) / 2),
                        (int)(parentBounds.Y + 100) // Offset from top of parent
                    );
                }
                else
                {
                    messageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }

                var stackPanel = new StackPanel
                {
                    Margin = new Thickness(20),
                    Background = new SolidColorBrush(Color.FromRgb(35, 36, 42)), // #23242a - dark theme background
                    Children =
                    {
                        new TextBlock()
                        {
                            Text = "❌ Error",
                            FontSize = 18,
                            FontWeight = FontWeight.Bold,
                            Foreground = Brushes.Red,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Margin = new Thickness(0, 0, 0, 15)
                        },
                        new TextBlock 
                        { 
                            Text = message,
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(0, 0, 0, 20),
                            FontSize = 14,
                            Foreground = Brushes.White, // White text on dark background
                            HorizontalAlignment = HorizontalAlignment.Center,
                            TextAlignment = TextAlignment.Center
                        },
                        new Button 
                        { 
                            Content = "OK",
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Width = 100,
                            Height = 35,
                            Background = Brushes.Red,
                            Foreground = Brushes.White,
                            FontSize = 14,
                            FontWeight = FontWeight.Bold,
                            Margin = new Thickness(0, 10, 0, 0)
                        }
                    }
                };

                messageBox.Content = stackPanel;
                
                // Handle the button click to close the message box
                if (stackPanel.Children.Count > 2 && stackPanel.Children[2] is Button button)
                {
                    button.Click += (sender, e) => messageBox.Close();
                }
                
                // Show the window
                messageBox.Show();
                
                // Wait for the window to be closed
                await WaitForWindowToClose(messageBox);
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error showing error notification: {ex.Message}", ex);
            }
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

        public bool ConfirmExit()
        {
            // This method will be called when user tries to logout/exit
            return !HasItems; // Return true if cart is empty (OK to exit)
        }

        public void Dispose()
        {
            if (_cartService != null)
            {
                _cartService.CartChanged -= OnCartChanged;
            }
            _logger?.Debug("CartViewModel disposed");
        }
    }
} 