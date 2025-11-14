using EbayClone.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace EbayClone.Web.Controllers;

public class AdminController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IHttpClientFactory httpClientFactory, ILogger<AdminController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IActionResult> Orders()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("EbayAPI");
            var response = await client.GetAsync("orders");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var orders = JsonSerializer.Deserialize<List<Order>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Order>();

                return View(orders);
            }

            return View(new List<Order>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all orders");
            return View(new List<Order>());
        }
    }

    [HttpPost]
    public async Task<IActionResult> ShipOrder(string orderId, string trackingNumber, string carrier)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("EbayAPI");
            var payload = new { TrackingNumber = trackingNumber, Carrier = carrier, OrderId = orderId };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"orders/{orderId}/ship", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Order shipped successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to ship order";
            }

            return RedirectToAction("Orders");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error shipping order {orderId}");
            TempData["Error"] = ex.Message;
            return RedirectToAction("Orders");
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateShippingStatus(string orderId, string status, string location, string description)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("EbayAPI");

            // Convert string status to enum value
            var statusValue = status switch
            {
                "Shipped" => 2,
                "InTransit" => 3,
                "OutForDelivery" => 4,
                "Delivered" => 5,
                "Failed" => 6,
                _ => 0
            };

            var payload = new { Status = statusValue, Location = location, Description = description };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"orders/{orderId}/shipping/update", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Shipping status updated successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to update shipping status";
            }

            return RedirectToAction("Orders");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating shipping status for order {orderId}");
            TempData["Error"] = ex.Message;
            return RedirectToAction("Orders");
        }
    }

    [HttpPost]
    public async Task<IActionResult> DisputeOrder(string orderId, string reason)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("EbayAPI");
            var payload = new { OrderId = orderId, Reason = reason };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"orders/{orderId}/dispute", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Dispute processed and refund initiated!";
                return RedirectToAction("OrderDetails", "Home", new { id = orderId });
            }
            else
            {
                TempData["Error"] = "Failed to process dispute";
                return RedirectToAction("OrderDetails", "Home", new { id = orderId });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error disputing order {orderId}");
            TempData["Error"] = ex.Message;
            return RedirectToAction("OrderDetails", "Home", new { id = orderId });
        }
    }
}