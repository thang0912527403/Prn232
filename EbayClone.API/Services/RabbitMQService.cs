using EbayClone.API.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EbayClone.API.Services;

public class RabbitMQService : IDisposable
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger<RabbitMQService> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMQService(IConnectionFactory connectionFactory, ILogger<RabbitMQService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        InitializeConnection();
    }

    private void InitializeConnection()
    {
        try
        {
            if (_connection == null || !_connection.IsOpen)
                _connection = _connectionFactory.CreateConnection();

            if (_channel == null || _channel.IsClosed)
                _channel = _connection.CreateModel();

            _channel.ExchangeDeclare("order.events", ExchangeType.Topic, durable: true);

            _channel.QueueDeclare("order.payment", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare("order.shipping", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare("order.email", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare("order.escrow", durable: true, exclusive: false, autoDelete: false);

            _channel.QueueBind("order.payment", "order.events", "order.payment.*");
            _channel.QueueBind("order.shipping", "order.events", "order.shipping.*");
            _channel.QueueBind("order.email", "order.events", "order.email.*");
            _channel.QueueBind("order.escrow", "order.events", "order.escrow.*");

            _logger.LogInformation("RabbitMQ connection established");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection. Retrying later.");
            _connection = null;
            _channel = null;
        }
    }

    public void PublishMessage<T>(string routingKey, T message)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = false
            };

            if (_channel == null || _channel.IsClosed)
                InitializeConnection();

            if (_channel == null)
            {
                _logger.LogWarning($"Cannot publish message. RabbitMQ channel is unavailable: {routingKey}");
                return;
            }

            var json = JsonSerializer.Serialize(message, options);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(
                exchange: "order.events",
                routingKey: routingKey,
                basicProperties: properties,
                body: body
            );

            _logger.LogInformation($"Published message to {routingKey}: {json}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to publish message to {routingKey}");
        }
    }

    // Sửa PublishOrder để tránh crash nếu order hoặc items null
    public void PublishOrder(Order? order, string routingKey)
    {
        try
        {
            if (order == null)
            {
                _logger.LogWarning("Order is null, skipping publish");
                return;
            }

            var items = order.Items ?? new List<OrderItem>();

            var dto = new OrderMessageDto
            {
                OrderId = order.OrderId?.ToString() ?? "",
                UserId = order.UserId?.ToString() ?? "",
                ShippingFee = order.ShippingFee,
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress ?? "",
                ShippingRegion = order.ShippingRegion ?? "",
                CreatedAt = order.CreatedAt,
                Items = items.Select(i => new OrderItemDto
                {
                    Id = i.Id?.ToString() ?? "",
                    ProductId = i.ProductId?.ToString() ?? "",
                    Price = i.Price,
                    Quantity = i.Quantity,
                    Subtotal = i.Subtotal
                }).ToList()
            };

            PublishMessage(routingKey, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to map order and publish message");
        }
    }

    public void StartConsuming(string queueName, Func<string, Task> onMessageReceived)
    {
        try
        {
            if (_channel == null || _channel.IsClosed)
                InitializeConnection();

            if (_channel == null)
            {
                _logger.LogError($"Cannot start consuming. RabbitMQ channel is unavailable for queue: {queueName}");
                return;
            }

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    await onMessageReceived(message);
                    _channel.BasicAck(ea.DeliveryTag, false);
                    _logger.LogInformation($"Message processed from {queueName}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing message from {queueName}");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            _logger.LogInformation($"Started consuming from {queueName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to start consuming from {queueName}");
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }

    public class OrderMessageDto
    {
        public string OrderId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public List<OrderItemDto> Items { get; set; } = new();
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = null!;
        public string ShippingRegion { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class OrderItemDto
    {
        public string Id { get; set; } = null!;
        public string ProductId { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
    }
}
