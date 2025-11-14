using EbayClone.API.Models;
using RabbitMQ.Client;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace EbayClone.API.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly RabbitMQService _rabbitMQ;
    public EmailService(IConfiguration configuration, ILogger<EmailService> logger, RabbitMQService rabbitMQ)
    {
        _configuration = configuration;
        _logger = logger;
        _rabbitMQ = rabbitMQ;
    }

    public async Task SendPaymentConfirmationEmailAsync(Order order)
    {
        try
        {
            var subject = $"Payment Confirmed - Order #{order.OrderId}";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Payment Successful!</h2>
                    <p>Dear Customer,</p>
                    <p>Your payment has been successfully processed.</p>
                    <h3>Order Details:</h3>
                    <ul>
                        <li><strong>Order ID:</strong> {order.OrderId}</li>
                        <li><strong>Total Amount:</strong> {order.TotalAmount:C}</li>
                        <li><strong>Payment Date:</strong> {order.PaidAt:yyyy-MM-dd HH:mm:ss}</li>
                        <li><strong>Transaction ID:</strong> {order.Payment?.TransactionId}</li>
                    </ul>
                    <h3>Items:</h3>
                    <table border='1' cellpadding='5' style='border-collapse: collapse;'>
                        <tr>
                            <th>Product</th>
                            <th>Quantity</th>
                            <th>Price</th>
                            <th>Subtotal</th>
                        </tr>";

            foreach (var item in order.Items)
            {
                body += $@"
                        <tr>
                            <td>{item.Product.Name}</td>
                            <td>{item.Quantity}</td>
                            <td>{item.Price:C}</td>
                            <td>{item.Subtotal:C}</td>
                        </tr>";
            }

            body += $@"
                    </table>
                    <p><strong>Shipping Fee:</strong> {order.ShippingFee:C}</p>
                    <p><strong>Discount:</strong> -{order.DiscountAmount:C}</p>
                    <h3><strong>Total:</strong> {order.TotalAmount:C}</h3>
                    <p><strong>Shipping Address:</strong> {order.ShippingAddress}</p>
                    <hr>
                    <p><em>Note: Your payment of {order.Escrow?.Amount:C} will be held in escrow for {order.Escrow?.EscrowPeriodDays} days until delivery is confirmed.</em></p>
                    <p>Thank you for shopping with us!</p>
                    <p>Best regards,<br>eBay Clone Team</p>
                </body>
                </html>";

            await SendEmailAsync(order.UserId, subject, body);
            _logger.LogInformation($"Payment confirmation email sent for order {order.OrderId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send payment confirmation email for order {order.OrderId}");
        }
    }

    public async Task SendOrderShippedEmailAsync(Order order)
    {
        try
        {
            var subject = $"Your Order Has Been Shipped - Order #{order.OrderId}";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Order Shipped!</h2>
                    <p>Dear Customer,</p>
                    <p>Good news! Your order has been shipped.</p>
                    <h3>Shipping Information:</h3>
                    <ul>
                        <li><strong>Order ID:</strong> {order.OrderId}</li>
                        <li><strong>Tracking Number:</strong> {order.Shipping?.TrackingNumber}</li>
                        <li><strong>Carrier:</strong> {order.Shipping?.Carrier}</li>
                        <li><strong>Shipped Date:</strong> {order.ShippedAt:yyyy-MM-dd HH:mm:ss}</li>
                        <li><strong>Estimated Delivery:</strong> {order.Shipping?.EstimatedDelivery:yyyy-MM-dd}</li>
                    </ul>
                    <p>You can track your package using the tracking number above.</p>
                    <p><strong>Shipping Address:</strong> {order.ShippingAddress}</p>
                    <hr>
                    <p><em>Your payment will remain in escrow until {order.Escrow?.ReleaseDate:yyyy-MM-dd}. After successful delivery, the funds will be released to the seller.</em></p>
                    <p>Thank you for your patience!</p>
                    <p>Best regards,<br>eBay Clone Team</p>
                </body>
                </html>";

            await SendEmailAsync(order.UserId, subject, body);
            _logger.LogInformation($"Order shipped email sent for order {order.OrderId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send order shipped email for order {order.OrderId}");
        }
    }

    public async Task SendOrderDeliveredEmailAsync(Order order)
    {
        try
        {
            var subject = $"Order Delivered - Order #{order.OrderId}";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Order Delivered Successfully!</h2>
                    <p>Dear Customer,</p>
                    <p>Your order has been delivered.</p>
                    <h3>Delivery Information:</h3>
                    <ul>
                        <li><strong>Order ID:</strong> {order.OrderId}</li>
                        <li><strong>Delivered Date:</strong> {order.DeliveredAt:yyyy-MM-dd HH:mm:ss}</li>
                        <li><strong>Tracking Number:</strong> {order.Shipping?.TrackingNumber}</li>
                    </ul>
                    <p>We hope you're satisfied with your purchase!</p>
                    <hr>
                    <p><em>The seller's payment of {order.Escrow?.Amount:C} will be released on {order.Escrow?.ReleaseDate:yyyy-MM-dd} ({order.Escrow?.EscrowPeriodDays} days from delivery). If you have any issues with the order, please contact us before this date.</em></p>
                    <p>Thank you for shopping with us!</p>
                    <p>Best regards,<br>eBay Clone Team</p>
                </body>
                </html>";

            await SendEmailAsync(order.UserId, subject, body);
            _logger.LogInformation($"Order delivered email sent for order {order.OrderId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send order delivered email for order {order.OrderId}");
        }
    }

    public async Task SendOrderFailedEmailAsync(Order order, string reason)
    {
        try
        {
            var subject = $"Delivery Failed - Order #{order.OrderId}";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Delivery Failed</h2>
                    <p>Dear Customer,</p>
                    <p>Unfortunately, the delivery of your order has failed.</p>
                    <h3>Order Information:</h3>
                    <ul>
                        <li><strong>Order ID:</strong> {order.OrderId}</li>
                        <li><strong>Tracking Number:</strong> {order.Shipping?.TrackingNumber}</li>
                        <li><strong>Reason:</strong> {reason}</li>
                    </ul>
                    <p>We're processing a refund for your order. The refund amount of {order.TotalAmount:C} will be returned to your PayPal account within 5-7 business days.</p>
                    <p>We apologize for the inconvenience.</p>
                    <p>Best regards,<br>eBay Clone Team</p>
                </body>
                </html>";

            await SendEmailAsync(order.UserId, subject, body);
            _logger.LogInformation($"Order failed email sent for order {order.OrderId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send order failed email for order {order.OrderId}");
        }
    }

    public async Task SendRefundConfirmationEmailAsync(Order order, string reason)
    {
        try
        {
            var subject = $"Refund Processed - Order #{order.OrderId}";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Refund Processed</h2>
                    <p>Dear Customer,</p>
                    <p>Your refund has been processed successfully.</p>
                    <h3>Refund Details:</h3>
                    <ul>
                        <li><strong>Order ID:</strong> {order.OrderId}</li>
                        <li><strong>Refund Amount:</strong> {order.TotalAmount:C}</li>
                        <li><strong>Reason:</strong> {reason}</li>
                        <li><strong>Processed Date:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}</li>
                    </ul>
                    <p>The refund will appear in your PayPal account within 5-7 business days.</p>
                    <p>We apologize for any inconvenience caused.</p>
                    <p>Best regards,<br>eBay Clone Team</p>
                </body>
                </html>";

            await SendEmailAsync(order.UserId, subject, body);
            _logger.LogInformation($"Refund confirmation email sent for order {order.OrderId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send refund confirmation email for order {order.OrderId}");
        }
    }

    public async Task SendEscrowReleaseEmailAsync(Order order)
    {
        try
        {
            var subject = $"Payment Released to Seller - Order #{order.OrderId}";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Payment Released</h2>
                    <p>Dear Customer,</p>
                    <p>The escrow period for your order has ended and the payment has been released to the seller.</p>
                    <h3>Order Information:</h3>
                    <ul>
                        <li><strong>Order ID:</strong> {order.OrderId}</li>
                        <li><strong>Amount Released:</strong> {order.Escrow?.Amount:C}</li>
                        <li><strong>Release Date:</strong> {order.Escrow?.ReleasedAt:yyyy-MM-dd HH:mm:ss}</li>
                    </ul>
                    <p>Thank you for your purchase and for being a valued customer!</p>
                    <p>Best regards,<br>eBay Clone Team</p>
                </body>
                </html>";

            await SendEmailAsync(order.UserId, subject, body);
            _logger.LogInformation($"Escrow release email sent for order {order.OrderId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send escrow release email for order {order.OrderId}");
        }
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            _logger.LogInformation($"Bắt đầu gửi email tới {to} với subject: {subject}");

            // For demo purposes, just log the email instead of actually sending
            // In production, use actual SMTP or email service like SendGrid

            _logger.LogInformation($@"
=== EMAIL SENT ===
To: {to}
Subject: {subject}
Body: {body.Substring(0, Math.Min(200, body.Length))}...
==================");

            // Uncomment below for actual email sending
            
            var smtpHost = "smtp.gmail.com";
            var smtpPort = 587;
            var fromEmail = "thang0912527403@gmail.com";
            var fromName = "Ebay Clone";
            var username = "thang0912527403@gmail.com";
            var password = "yzfl bgme xcpz apzy";

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(username, password)
            };

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(to);

            await client.SendMailAsync(message);
            

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email");
            throw;
        }
    }
    public async Task PublishEmailMessageAsync(EmailMessage email)
    {
        _rabbitMQ.PublishMessage("order.email.*", email);
        await Task.CompletedTask;
    }

}