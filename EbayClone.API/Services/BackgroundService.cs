using EbayClone.API.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using EbayClone.API.Models;
namespace EbayClone.API;

// Background service to cancel expired orders
public class OrderTimeoutBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderTimeoutBackgroundService> _logger;

    public OrderTimeoutBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<OrderTimeoutBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Order Timeout Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();

                await orderService.CancelExpiredOrdersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Order Timeout Background Service");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}

// Background service to release escrow funds
public class EscrowReleaseBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EscrowReleaseBackgroundService> _logger;

    public EscrowReleaseBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<EscrowReleaseBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Escrow Release Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();

                await orderService.ProcessEscrowReleasesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Escrow Release Background Service");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}

public class EmailConsumerService : BackgroundService
{
    private readonly RabbitMQService _rabbit;
    private readonly EmailService _email;
    private readonly ILogger<EmailConsumerService> _logger;

    public EmailConsumerService(RabbitMQService rabbit, EmailService email, ILogger<EmailConsumerService> logger)
    {
        _rabbit = rabbit;
        _email = email;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailConsumerService started");

        _rabbit.StartConsuming("order.email", async message =>
        {
            var data = JsonSerializer.Deserialize<EmailMessage>(message);
            await _email.SendEmailAsync(data.To, data.Subject, data.Body);
        });

        return Task.CompletedTask;
    }
}
