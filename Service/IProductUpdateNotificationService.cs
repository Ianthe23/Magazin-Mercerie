using System;

namespace magazin_mercerie.Service
{
    /*
    * This interface is used to notify the views about the changes in the products and orders.
    * It is used to update the views about the changes in the products and orders.
    */
    public interface IProductUpdateNotificationService
    {
        event EventHandler<ProductQuantityUpdatedEventArgs>? ProductQuantityUpdated;
        event EventHandler? ProductsChanged;
        event EventHandler<OrderStatusUpdatedEventArgs>? OrderStatusUpdated;

        void NotifyProductQuantityUpdated(Guid productId, decimal newQuantity);
        void NotifyProductsChanged();
        void NotifyOrderStatusUpdated(Guid orderId, Guid clientId, Status newStatus, string clientName);
    }
    
    public class ProductQuantityUpdatedEventArgs : EventArgs
    {
        public Guid ProductId { get; set; }
        public decimal NewQuantity { get; set; }
        
        public ProductQuantityUpdatedEventArgs(Guid productId, decimal newQuantity)
        {
            ProductId = productId;
            NewQuantity = newQuantity;
        }
    }
    
    public class OrderStatusUpdatedEventArgs : EventArgs
    {
        public Guid OrderId { get; set; }
        public Guid ClientId { get; set; }
        public Status NewStatus { get; set; }
        public string ClientName { get; set; }
        
        public OrderStatusUpdatedEventArgs(Guid orderId, Guid clientId, Status newStatus, string clientName)
        {
            OrderId = orderId;
            ClientId = clientId;
            NewStatus = newStatus;
            ClientName = clientName;
        }
    }
} 