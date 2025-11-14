using EbayClone.API.Models;

namespace EbayClone.API.Services;

public class EscrowService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EscrowService> _logger;
    private readonly PaymentService _paymentService;
    private readonly EmailService _emailService;
    //private readonly RabbitMQService _rabbitMQService;

    public EscrowService(
        IConfiguration configuration,
        ILogger<EscrowService> logger,
        PaymentService paymentService,
        EmailService emailService
        //,RabbitMQService rabbitMQService
        )
    {
        _configuration = configuration;
        _logger = logger;
        _paymentService = paymentService;
        _emailService = emailService;
        //_rabbitMQService = rabbitMQService;
    }

    public EscrowInfo CreateEscrow(Order order)
    {
        var escrowPeriodDays = 21;
        var heldDate = DateTime.UtcNow;
        var releaseDate = heldDate.AddDays(escrowPeriodDays);

        var escrow = new EscrowInfo
        {
            Amount = order.TotalAmount,
            Status = EscrowStatus.Held,
            HeldDate = heldDate,
            ReleaseDate = releaseDate,
            EscrowPeriodDays = escrowPeriodDays
        };

        _logger.LogInformation($"Escrow created for order {order.OrderId}: Amount={escrow.Amount:C}, ReleaseDate={releaseDate:yyyy-MM-dd}, Period={escrowPeriodDays} days");

        return escrow;
    }

    public int CalculateEscrowPeriod(decimal sellerRating)
    {
        // Calculate escrow period based on seller rating
        // Higher rating = shorter escrow period
        var basePeriod = _configuration.GetValue<int>("OrderSettings:EscrowPeriodDays", 21);

        if (sellerRating >= 4.8m)
            return Math.Max(7, basePeriod - 14); // 7 days for top sellers
        else if (sellerRating >= 4.5m)
            return Math.Max(14, basePeriod - 7); // 14 days for good sellers
        else
            return basePeriod; // 21 days for new/lower rated sellers
    }

    public async Task<bool> ReleaseEscrowAsync(Order order)
    {
        try
        {
            if (order.Escrow == null || order.Escrow.Status != EscrowStatus.Held)
            {
                _logger.LogWarning($"Cannot release escrow for order {order.OrderId}: Invalid escrow status");
                return false;
            }

            order.Escrow.Status = EscrowStatus.Released;
            order.Escrow.ReleasedAt = DateTime.UtcNow;

            _logger.LogInformation($"Escrow released for order {order.OrderId}: Amount={order.Escrow.Amount:C}");

            // Send notification to seller
            await _emailService.SendEscrowReleaseEmailAsync(order);

            // Publish event
            //_rabbitMQService.PublishMessage("order.escrow.released", new
            //{
            //    OrderId = order.OrderId,
            //    //SellerId = order.SellerId,
            //    Amount = order.Escrow.Amount,
            //    ReleasedAt = order.Escrow.ReleasedAt
            //});

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to release escrow for order {order.OrderId}");
            return false;
        }
    }

    public async Task<bool> RefundEscrowAsync(Order order, string reason)
    {
        try
        {
            if (order.Escrow == null || order.Escrow.Status != EscrowStatus.Held)
            {
                _logger.LogWarning($"Cannot refund escrow for order {order.OrderId}: Invalid escrow status");
                return false;
            }

            if (order.Payment == null || string.IsNullOrEmpty(order.Payment.PayPalCaptureId))
            {
                _logger.LogError($"Cannot refund order {order.OrderId}: Missing payment information");
                return false;
            }

            // Process refund through PayPal
            var refundSuccess = await _paymentService.RefundPaymentAsync(
                order.Payment.PayPalCaptureId,
                order.Escrow.Amount
            );

            if (!refundSuccess)
            {
                _logger.LogError($"PayPal refund failed for order {order.OrderId}");
                return false;
            }

            order.Escrow.Status = EscrowStatus.Refunded;
            order.Escrow.RefundReason = reason;
            order.Escrow.ReleasedAt = DateTime.UtcNow;
            order.Status = OrderStatus.Refunded;

            _logger.LogInformation($"Escrow refunded for order {order.OrderId}: Amount={order.Escrow.Amount:C}, Reason={reason}");

            // Send notification to buyer
            await _emailService.SendRefundConfirmationEmailAsync(order, reason);

            // Publish event
            //_rabbitMQService.PublishMessage("order.escrow.refunded", new
            //{
            //    OrderId = order.OrderId,
            //    UserId = order.UserId,
            //    Amount = order.Escrow.Amount,
            //    Reason = reason,
            //    RefundedAt = order.Escrow.ReleasedAt
            //});

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to refund escrow for order {order.OrderId}");
            return false;
        }
    }

    public bool ShouldReleaseEscrow(Order order)
    {
        if (order.Escrow == null || order.Escrow.Status != EscrowStatus.Held)
            return false;

        if (order.Status != OrderStatus.Delivered)
            return false;

        return DateTime.UtcNow >= order.Escrow.ReleaseDate;
    }
}