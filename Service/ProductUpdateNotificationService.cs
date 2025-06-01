using System;
using log4net;

namespace magazin_mercerie.Service
{
    public class ProductUpdateNotificationService : IProductUpdateNotificationService
    {
        private readonly ILog _logger;
        
        public event EventHandler<ProductQuantityUpdatedEventArgs>? ProductQuantityUpdated;
        public event EventHandler? ProductsChanged;
        public event EventHandler<OrderStatusUpdatedEventArgs>? OrderStatusUpdated;
        
        public ProductUpdateNotificationService()
        {
            _logger = LogManager.GetLogger(typeof(ProductUpdateNotificationService));
            _logger.Debug("ProductUpdateNotificationService initialized");
        }
        
        public void NotifyProductQuantityUpdated(Guid productId, decimal newQuantity)
        {
            _logger.Info($"🔔 TRIGGERING NOTIFICATION: Product {productId} quantity updated to {newQuantity}");
            
            var eventArgs = new ProductQuantityUpdatedEventArgs(productId, newQuantity);
            var handlers = ProductQuantityUpdated?.GetInvocationList();
            
            if (handlers != null && handlers.Length > 0)
            {
                _logger.Info($"📡 Broadcasting to {handlers.Length} subscribers");
                ProductQuantityUpdated?.Invoke(this, eventArgs);
            }
            else
            {
                _logger.Warn($"⚠️ NO SUBSCRIBERS: No views are listening for product quantity updates!");
            }
        }
        
        public void NotifyProductsChanged()
        {
            _logger.Debug("Triggering products changed notification");
            ProductsChanged?.Invoke(this, EventArgs.Empty);
        }
        
        public void NotifyOrderStatusUpdated(Guid orderId, Guid clientId, Status newStatus, string clientName)
        {
            _logger.Info($"🔔 TRIGGERING ORDER STATUS NOTIFICATION: Order {orderId} for client {clientName} updated to {newStatus}");
            
            var eventArgs = new OrderStatusUpdatedEventArgs(orderId, clientId, newStatus, clientName);
            var handlers = OrderStatusUpdated?.GetInvocationList();
            
            if (handlers != null && handlers.Length > 0)
            {
                _logger.Info($"📡 Broadcasting order status update to {handlers.Length} subscribers");
                OrderStatusUpdated?.Invoke(this, eventArgs);
            }
            else
            {
                _logger.Warn($"⚠️ NO SUBSCRIBERS: No views are listening for order status updates!");
            }
        }
    }
} 