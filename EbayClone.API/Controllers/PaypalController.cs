// Controllers/PayPalController.cs
using EbayClone.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
namespace EbayClone.API.Services;
[ApiController]
[Route("api/paypal")]
public class PaypalController : ControllerBase
{
    public class PaypalOrderRequest
    {
        public string orderId { get; set; } = string.Empty;
        public string paypalOrderId { get; set; } = string.Empty;
    }
    public class PaypalRequest
    {
        public string value { get; set; } = string.Empty;
        public string orderId { get; set; } = string.Empty;
    }
    private readonly EbayDbContext _db;
    private readonly PaypalService _paypal;
    private readonly ILogger<PaypalController> _log;
    private readonly string _ppclienId;
    private readonly string _ppclientsecret;
    private readonly IConfiguration _configuration;
    private readonly OrderService _orderService;
    private readonly EmailService _emailService;
    public PaypalController(EmailService emailService,OrderService orderService,IConfiguration configuration,EbayDbContext db, PaypalService paypal, ILogger<PaypalController> log)
    {
        _configuration = configuration;
        _ppclienId = configuration["PayPal:ClientId"] ?? "";
        _ppclientsecret = configuration["PayPal:ClientSecret"] ?? "";
        _db = db;
        _paypal = paypal;
        _log = log;
        _orderService = orderService;
        _emailService = emailService;
    }
    [HttpPost("createpaypalorder")]
    public async Task<IActionResult> Paypalcheckout([FromBody] PaypalRequest request)
    {
        try { 
            var response= await _paypal.CreateOrderAsync(request.value);
            Console.WriteLine("Create PayPal Order called");
            return Ok(new { id = response.id });
        } 
        catch (Exception ex) 
        { 
            _log.LogError(ex, "Error creating PayPal order"); 
            return StatusCode(500, new { error = ex.Message }); 
        }
    }
    [HttpPost("capturepaypalorder")]
    public async Task<IActionResult> CaptureOrder([FromBody] PaypalOrderRequest order)
    {
        try
        {
            var captureResult = await _paypal.CaptureOrderAsync(order.paypalOrderId);
            await _orderService.CompletePaymentAsync(order.orderId, order.paypalOrderId);
            await _emailService.PublishEmailMessageAsync(new EmailMessage
            {
                To = "hoan0912527403@gmail.com",
                Subject = $"Payment Confirmed - Order #{order.orderId}",
                Body = "Payment successful"
            });
            return Ok(new
            {
                success = true,
                payer = new
                {
                    name = new
                    {
                        given_name = captureResult["payer"]?["name"]?["given_name"]?.ToString() ?? "Customer"
                    }
                },
                status = captureResult["status"]?.ToString() ?? "COMPLETED",
                id = captureResult["id"]?.ToString()
            });
        }
        catch (Exception ex)    
        {
            _log.LogError(ex, "Error capture PayPal order");
            return StatusCode(500, new { error = ex.Message });
        }
    }


    // Manual trigger for release (admin)
    [HttpPost("release-escrow/{orderId}")]
    public async Task<IActionResult> ReleaseEscrow(string orderId)
    {
        var order = await _db.Orders.Include(o => o.Escrow).Include(o => o.User).FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (order == null || order.Escrow == null) return NotFound();

        if (order.Escrow.Status != EscrowStatus.Held || order.Escrow.ReleaseDate > DateTime.UtcNow)
            return BadRequest("Escrow not ready");

        // Assuming seller email stored in User.Email
        var sellerEmail = order.User?.Email;
        if (string.IsNullOrEmpty(sellerEmail)) return BadRequest("Seller has no paypal email");

        var payout = await _paypal.CreatePayoutAsync(sellerEmail, order.Escrow.Amount, "USD", $"Payout for order {order.OrderId}");

        // On success, update escrow
        order.Escrow.Status = EscrowStatus.Released;
        order.Escrow.ReleasedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(payout);
    }
}
