using EbayClone.API.Models;
using Polly;
using Polly.Retry;

namespace EbayClone.API.Services;

public class ShippingService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ShippingService> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public ShippingService(HttpClient httpClient, IConfiguration configuration, ILogger<ShippingService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        var retryCount = _configuration.GetValue<int>("Shipping:RetryCount", 3);
        var retryDelay = _configuration.GetValue<int>("Shipping:RetryDelaySeconds", 2);

        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt => TimeSpan.FromSeconds(retryDelay * retryAttempt),
                (exception, timeSpan, retry, context) =>
                {
                    _logger.LogWarning($"Shipping API call failed. Retry {retry}/{retryCount} after {timeSpan.TotalSeconds}s. Error: {exception.Message}");
                }
            );
    }

    public async Task<(bool Success, string TrackingNumber, string ErrorMessage)> CreateShipmentAsync(Order order)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var transactionId = Guid.NewGuid().ToString();

                // Simulate API call to shipping provider
                await Task.Delay(500); // Simulate network delay

                // Mock tracking number generation
                var trackingNumber = $"VN{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";

                _logger.LogInformation($"Shipment created - OrderId: {order.OrderId}, TrackingNumber: {trackingNumber}, TransactionId: {transactionId}");

                return (true, trackingNumber, string.Empty);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to create shipment for order {order.OrderId} after retries");
            return (false, string.Empty, ex.Message);
        }
    }

    public async Task<(bool Success, ShippingInfo ShippingInfo, string ErrorMessage)> GetShippingStatusAsync(string trackingNumber)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                // Simulate API call
                await Task.Delay(300);

                var shippingInfo = new ShippingInfo
                {
                    TrackingNumber = trackingNumber,
                    Carrier = "Vietnam Post",
                    Status = ShippingStatus.InTransit,
                    ShippedDate = DateTime.UtcNow.AddHours(-2),
                    EstimatedDelivery = DateTime.UtcNow.AddDays(3),
                    Events = new List<ShippingEvent>
                    {
                        new ShippingEvent
                        {
                            Timestamp = DateTime.UtcNow.AddHours(-2),
                            Status = "Picked Up",
                            Location = "Hanoi Hub",
                            Description = "Package picked up from seller"
                        },
                        new ShippingEvent
                        {
                            Timestamp = DateTime.UtcNow.AddHours(-1),
                            Status = "In Transit",
                            Location = "Regional Sorting Center",
                            Description = "Package in transit to destination"
                        }
                    }
                };

                _logger.LogInformation($"Retrieved shipping status for tracking: {trackingNumber}");
                return (true, shippingInfo, string.Empty);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get shipping status for {trackingNumber}");
            return (false, new ShippingInfo(), ex.Message);
        }
    }

    public async Task<bool> UpdateShippingStatusAsync(string trackingNumber, ShippingStatus status, string location, string description)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var transactionId = Guid.NewGuid().ToString();

                // Simulate API call
                await Task.Delay(300);

                _logger.LogInformation($"Shipping status updated - Tracking: {trackingNumber}, Status: {status}, TransactionId: {transactionId}");
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to update shipping status for {trackingNumber}");
            return false;
        }
    }

    public async Task<bool> CancelShipmentAsync(string trackingNumber)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var transactionId = Guid.NewGuid().ToString();

                // Simulate API call
                await Task.Delay(300);

                _logger.LogInformation($"Shipment cancelled - Tracking: {trackingNumber}, TransactionId: {transactionId}");
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to cancel shipment {trackingNumber}");
            return false;
        }
    }

    public DateTime CalculateEstimatedDelivery(string region)
    {
        var daysToAdd = region.ToLower() switch
        {
            "hanoi" => 2,
            "hochiminh" => 3,
            "danang" => 3,
            "domestic" => 5,
            "international" => 15,
            _ => 5
        };

        return DateTime.UtcNow.AddDays(daysToAdd);
    }
}