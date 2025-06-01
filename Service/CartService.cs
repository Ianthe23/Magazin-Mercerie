using System;
using System.Collections.ObjectModel;
using System.Linq;
using log4net;
using magazin_mercerie.Models;

namespace magazin_mercerie.Service
{
    public class CartService : ICartService
    {
        private readonly ILog _logger;
        
        public ObservableCollection<CartItem> CartItems { get; private set; }
        
        public int TotalItemCount => CartItems.Sum(item => item.Quantity);
        
        public decimal TotalPrice => CartItems.Sum(item => item.TotalPrice);
        
        public bool HasItems => CartItems.Count > 0;
        
        public event EventHandler CartChanged;
        
        public CartService()
        {
            _logger = LogManager.GetLogger(typeof(CartService));
            CartItems = new ObservableCollection<CartItem>();
            _logger?.Info("CartService initialized");
        }
        
        public void AddToCart(Produs product, int quantity = 1)
        {
            try
            {
                if (product == null)
                {
                    _logger?.Warn("Attempted to add null product to cart");
                    return;
                }
                
                if (quantity <= 0)
                {
                    _logger?.Warn($"Invalid quantity {quantity} for product {product.Nume}");
                    return;
                }
                
                var existingItem = GetCartItem(product);
                
                if (existingItem != null)
                {
                    // Update quantity if item already exists
                    var newQuantity = existingItem.Quantity + quantity;
                    if (newQuantity <= product.Cantitate)
                    {
                        existingItem.Quantity = newQuantity;
                        _logger?.Info($"Updated quantity for {product.Nume} to {newQuantity}");
                    }
                    else
                    {
                        _logger?.Warn($"Cannot add {quantity} more of {product.Nume} - would exceed stock ({product.Cantitate})");
                        return;
                    }
                }
                else
                {
                    // Add new item
                    if (quantity <= product.Cantitate)
                    {
                        CartItems.Add(new CartItem(product, quantity));
                        _logger?.Info($"Added {quantity} of {product.Nume} to cart");
                    }
                    else
                    {
                        _logger?.Warn($"Cannot add {quantity} of {product.Nume} - exceeds stock ({product.Cantitate})");
                        return;
                    }
                }
                
                OnCartChanged();
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error adding product to cart: {ex.Message}", ex);
            }
        }
        
        public void RemoveFromCart(Produs product)
        {
            try
            {
                var item = GetCartItem(product);
                if (item != null)
                {
                    CartItems.Remove(item);
                    _logger?.Info($"Removed {product.Nume} from cart");
                    OnCartChanged();
                }
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error removing product from cart: {ex.Message}", ex);
            }
        }
        
        public void UpdateQuantity(Produs product, int newQuantity)
        {
            try
            {
                var item = GetCartItem(product);
                if (item != null)
                {
                    if (newQuantity <= 0)
                    {
                        RemoveFromCart(product);
                    }
                    else if (newQuantity <= product.Cantitate)
                    {
                        item.Quantity = newQuantity;
                        _logger?.Info($"Updated {product.Nume} quantity to {newQuantity}");
                        OnCartChanged();
                    }
                    else
                    {
                        _logger?.Warn($"Cannot set quantity to {newQuantity} for {product.Nume} - exceeds stock ({product.Cantitate})");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error updating product quantity: {ex.Message}", ex);
            }
        }
        
        public void ClearCart()
        {
            try
            {
                var itemCount = CartItems.Count;
                CartItems.Clear();
                _logger?.Info($"Cleared cart - removed {itemCount} items");
                OnCartChanged();
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error clearing cart: {ex.Message}", ex);
            }
        }
        
        public CartItem GetCartItem(Produs product)
        {
            return CartItems.FirstOrDefault(item => item.Product.Id == product.Id);
        }
        
        private void OnCartChanged()
        {
            CartChanged?.Invoke(this, EventArgs.Empty);
        }
    }
} 