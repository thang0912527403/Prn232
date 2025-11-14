using EbayClone.API.Models;
using EbayClone.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EbayClone.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly ILogger<OrdersController> _logger;
    private readonly EmailService _emailService;
    public OrdersController(OrderService orderService, ILogger<OrdersController> logger, EmailService emailService)
    {
        _orderService = orderService;
        _logger = logger;
        _emailService = emailService;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var order = await _orderService.CreateOrderAsync(request);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{orderId}/payment/initiate")]
    public async Task<ActionResult> InitiatePayment(string orderId)
    {
        try
        {
            var (success, paypalOrderId, errorMessage) = await _orderService.InitiatePaymentAsync(orderId);

            if (!success)
            {
                return BadRequest(new { error = errorMessage });
            }

            return Ok(new { paypalOrderId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error initiating payment for order {orderId}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

   
    [HttpPost("{orderId}/ship")]
    public async Task<ActionResult> ShipOrder(string orderId, [FromBody] UpdateShippingRequest request)
    {
        try
        {
            var success = await _orderService.ShipOrderAsync(orderId, request.TrackingNumber, request.Carrier);

            if (!success)
            {
                return BadRequest(new { error = "Failed to ship order" });
            }

            var order = _orderService.GetOrder(orderId);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error shipping order {orderId}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{orderId}/shipping/update")]
    public async Task<ActionResult> UpdateShippingStatus(
        string orderId,
        [FromBody] UpdateShippingStatusRequest request)
    {
        try
        {
            var success = await _orderService.UpdateShippingStatusAsync(
                orderId,
                request.Status,
                request.Location,
                request.Description
            );

            if (!success)
            {
                return BadRequest(new { error = "Failed to update shipping status" });
            }

            var order = _orderService.GetOrder(orderId);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating shipping status for order {orderId}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{orderId}/dispute")]
    public async Task<ActionResult> DisputeOrder(string orderId, [FromBody] DisputeRequest request)
    {
        try
        {
            var success = await _orderService.DisputeOrderAsync(orderId, request.Reason);

            if (!success)
            {
                return BadRequest(new { error = "Failed to process dispute" });
            }

            var order = _orderService.GetOrder(orderId);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error disputing order {orderId}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{orderId}")]
    public ActionResult<Order> GetOrder(string orderId)
    {
        var order = _orderService.GetOrder(orderId);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    [HttpGet("user/{userId}")]
    public ActionResult<IEnumerable<Order>> GetUserOrders(string userId)
    {
        var orders = _orderService.GetUserOrders(userId);
        return Ok(orders);
    }

    [HttpGet]
    public ActionResult<IEnumerable<Order>> GetAllOrders()
    {
        var orders = _orderService.GetAllOrders();
        return Ok(orders);
    }
}

public class UpdateShippingStatusRequest
{
    public ShippingStatus Status { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}