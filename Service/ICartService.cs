using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using magazin_mercerie.Models;

namespace magazin_mercerie.Service
{
    public interface ICartService
    {
        ObservableCollection<CartItem> CartItems { get; }
        int TotalItemCount { get; }
        decimal TotalPrice { get; }
        bool HasItems { get; }
        
        event EventHandler CartChanged;
        
        void AddToCart(Produs product, int quantity = 1);
        void RemoveFromCart(Produs product);
        void UpdateQuantity(Produs product, int newQuantity);
        void ClearCart();
        CartItem GetCartItem(Produs product);
    }
} 