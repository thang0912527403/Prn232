using EbayClone.API.Models;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
namespace EbayClone.API.Services;

public class OrderService
{
    private readonly ConcurrentDictionary<string, Order> _orders = new();
    private readonly PaymentService _paymentService;
    private readonly ShippingService _shippingService;
    private readonly EmailService _emailService;
    private readonly EscrowService _escrowService;
    //private readonly RabbitMQService _rabbitMQService;
    private readonly ILogger<OrderService> _logger;
    private readonly EbayDbContext _dbContext;
    public OrderService(
        PaymentService paymentService,
        ShippingService shippingService,
        EmailService emailService,
        EscrowService escrowService,
        //RabbitMQService rabbitMQService,
        ILogger<OrderService> logger,
        EbayDbContext dbContext)
    {
        _paymentService = paymentService;
        _shippingService = shippingService;
        _emailService = emailService;
        _escrowService = escrowService;
        //_rabbitMQService = rabbitMQService;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        var order = new Order();

        order.UserId = request.UserId;
        foreach(var item in request.Items)
        {
            item.OrderId= order.OrderId;
        }
        order.Items = request.Items;
        order.ShippingAddress = request.ShippingAddress;
        order.ShippingRegion = request.ShippingRegion;
        order.CouponCode = request.CouponCode;
        order.Status = OrderStatus.PendingPayment;
       

        // Calculate amounts
        var itemsTotal = order.Items.Sum(i => i.Subtotal);
        order.ShippingFee = _paymentService.CalculateShippingFee(order.ShippingRegion);
        order.DiscountAmount = _paymentService.ApplyDiscount(itemsTotal, order.CouponCode);
        order.TotalAmount = itemsTotal + order.ShippingFee - order.DiscountAmount;

        _orders.TryAdd(order.OrderId, order);
        await _dbContext.Orders.AddAsync(order);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation($"Order created: {order.OrderId}, Total: {order.TotalAmount:C}");

        // Publish order created event
        //_rabbitMQService.PublishMessage("order.created", order);

        return order;
    }

    public async Task<(bool Success, string PayPalOrderId, string ErrorMessage)> InitiatePaymentAsync(string orderId)
    {
        if (!_orders.TryGetValue(orderId, out var order))
        {
            return (false, string.Empty, "Order not found");
        }

        if (order.Status != OrderStatus.PendingPayment)
        {
            return (false, string.Empty, "Order is not pending payment");
        }

        // Create PayPal order
        var (success, paypalOrderId, errorMessage) = await _paymentService.CreatePayPalOrderAsync(order.TotalAmount);

        if (success)
        {
            order.Payment = new PaymentInfo
            {
                TransactionId = Guid.NewGuid().ToString(),
                PayPalOrderId = paypalOrderId,
                Status = PaymentStatus.Pending
            };

            _logger.LogInformation($"Payment initiated for order {orderId}: PayPalOrderId={paypalOrderId}");
        }

        return (success, paypalOrderId, errorMessage);
    }

    public async Task CompletePaymentAsync(string orderId, string paypalOrderId)
    {
        var order =await _dbContext.Orders.Include(o => o.Payment).Include(o => o.Escrow).FirstAsync(o=>o.OrderId==orderId);
        if (order==null)
        {
            _logger.LogError($"Order not found: {orderId}");
        }

        PaymentInfo payment = new PaymentInfo
        {
            OrderId = orderId,
            TransactionId = Guid.NewGuid().ToString(),
            PayPalOrderId = paypalOrderId,
            Status = PaymentStatus.Completed,
            ProcessedAt = DateTime.UtcNow,
            PayPalCaptureId = paypalOrderId
        };
        // Update order
        order.Status = OrderStatus.Paid;
        order.PaidAt = DateTime.UtcNow;
        order.Payment = payment;
        order.Payment.PayPalCaptureId = paypalOrderId;
        order.Payment.ProcessedAt = DateTime.UtcNow;

        // Create escrow
        order.Escrow = _escrowService.CreateEscrow(order);

        _logger.LogInformation($"Payment completed for order {orderId}: CaptureId={paypalOrderId}");
        await _dbContext.SaveChangesAsync();    
        // Send confirmation email
        await _emailService.SendPaymentConfirmationEmailAsync(order);

        // Publish payment completed event
        //_rabbitMQService.PublishMessage("order.payment.completed", order);

    }

    public async Task<bool> ShipOrderAsync(string orderId, string trackingNumber, string carrier)
    {
        if (!_orders.TryGetValue(orderId, out var order))
        {
            return false;
        }

        if (order.Status != OrderStatus.Paid && order.Status != OrderStatus.Processing)
        {
            _logger.LogWarning($"Cannot ship order {orderId}: Invalid status {order.Status}");
            return false;
        }

        order.Status = OrderStatus.Shipped;
        order.ShippedAt = DateTime.UtcNow;
        order.Shipping = new ShippingInfo
        {
            TrackingNumber = trackingNumber,
            Carrier = carrier,
            Status = ShippingStatus.Shipped,
            ShippedDate = DateTime.UtcNow,
            EstimatedDelivery = _shippingService.CalculateEstimatedDelivery(order.ShippingRegion)
        };

        _logger.LogInformation($"Order shipped: {orderId}, Tracking: {trackingNumber}");

        // Send email notification
        await _emailService.SendOrderShippedEmailAsync(order);

        // Publish shipping event
        //_rabbitMQService.PublishMessage("order.shipping.shipped", order);

        return true;
    }

    public async Task<bool> UpdateShippingStatusAsync(string orderId, ShippingStatus status, string location, string description)
    {
        if (!_orders.TryGetValue(orderId, out var order))
        {
            return false;
        }

        if (order.Shipping == null)
        {
            return false;
        }

        order.Shipping.Status = status;
        order.Shipping.Events.Add(new ShippingEvent
        {
            Timestamp = DateTime.UtcNow,
            Status = status.ToString(),
            Location = location,
            Description = description
        });

        if (status == ShippingStatus.Delivered)
        {
            order.Status = OrderStatus.Delivered;
            order.DeliveredAt = DateTime.UtcNow;
            order.Shipping.ActualDelivery = DateTime.UtcNow;

            await _emailService.SendOrderDeliveredEmailAsync(order);
            //_rabbitMQService.PublishMessage("order.shipping.delivered", order);

            _logger.LogInformation($"Order delivered: {orderId}");
        }
        else if (status == ShippingStatus.Failed)
        {
            await HandleFailedDeliveryAsync(order, description);
        }

        return true;
    }

    public async Task<bool> DisputeOrderAsync(string orderId, string reason)
    {
        if (!_orders.TryGetValue(orderId, out var order))
        {
            return false;
        }

        if (order.Status != OrderStatus.Delivered && order.Status != OrderStatus.Shipped)
        {
            _logger.LogWarning($"Cannot dispute order {orderId}: Invalid status {order.Status}");
            return false;
        }

        order.Status = OrderStatus.Disputed;

        // Process refund
        var refundSuccess = await _escrowService.RefundEscrowAsync(order, reason);

        if (refundSuccess)
        {
            _logger.LogInformation($"Order dispute processed: {orderId}, Reason: {reason}");
            //_rabbitMQService.PublishMessage("order.disputed", order);
            return true;
        }

        return false;
    }

    private async Task HandleFailedDeliveryAsync(Order order, string reason)
    {
        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;

        await _emailService.SendOrderFailedEmailAsync(order, reason);
        await _escrowService.RefundEscrowAsync(order, $"Delivery failed: {reason}");

        _logger.LogWarning($"Order delivery failed: {order.OrderId}, Reason: {reason}");
    }

    public async Task CancelExpiredOrdersAsync()
    {
        var timeoutMinutes = 30; // From configuration
        var expiredOrders = _orders.Values
            .Where(o => o.Status == OrderStatus.PendingPayment &&
                       DateTime.UtcNow - o.CreatedAt > TimeSpan.FromMinutes(timeoutMinutes))
            .ToList();

        foreach (var order in expiredOrders)
        {
            order.Status = OrderStatus.Cancelled;
            order.CancelledAt = DateTime.UtcNow;

            _logger.LogInformation($"Order cancelled due to timeout: {order.OrderId}");
            //_rabbitMQService.PublishMessage("order.cancelled.timeout", order);
        }
    }

    public async Task ProcessEscrowReleasesAsync()
    {
        var ordersToRelease = _orders.Values
            .Where(o => _escrowService.ShouldReleaseEscrow(o))
            .ToList();

        foreach (var order in ordersToRelease)
        {
            await _escrowService.ReleaseEscrowAsync(order);
        }
    }

    public Order? GetOrder(string orderId)
    {
        return _dbContext.Orders.Include(o => o.Items).FirstOrDefault(o => o.OrderId == orderId);
    }

    public IEnumerable<Order> GetUserOrders(string userId)
    {
        return _orders.Values.Where(o => o.UserId == userId).OrderByDescending(o => o.CreatedAt);
    }

    public IEnumerable<Order> GetAllOrders()
    {
        return _orders.Values.OrderByDescending(o => o.CreatedAt);
    }
}