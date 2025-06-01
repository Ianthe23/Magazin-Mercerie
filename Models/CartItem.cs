using System;
using magazin_mercerie.ViewModels;

namespace magazin_mercerie.Models
{
    public class CartItem : ViewModelBase
    {
        public Produs Product { get; set; }
        
        private int _quantity;
        public int Quantity 
        { 
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPrice));
            }
        }
        
        public decimal TotalPrice => Product.Pret * Quantity;
        
        public CartItem(Produs product, int quantity = 1)
        {
            Product = product ?? throw new ArgumentNullException(nameof(product));
            Quantity = quantity;
        }
        
        public void IncreaseQuantity()
        {
            if (Quantity < Product.Cantitate) // Don't exceed stock
            {
                Quantity++;
            }
        }
        
        public void DecreaseQuantity()
        {
            if (Quantity > 1)
            {
                Quantity--;
            }
        }
    }
} 