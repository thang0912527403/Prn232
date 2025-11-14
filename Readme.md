# eBay Clone - E-commerce Platform with Escrow Payment System

A complete e-commerce platform built with ASP.NET Core featuring PayPal integration, escrow payment system, and shipping management.

## 🚀 Features

### Payment & Escrow System
- ✅ **PayPal Sandbox Integration** - Real PayPal payment processing
- ✅ **Escrow Protection** - Funds held for 7-21 days based on seller rating
- ✅ **Automatic Refunds** - Dispute handling with automatic refund processing
- ✅ **Payment Timeout** - Auto-cancel orders after 30 minutes without payment

### Shipping & Delivery
- ✅ **Multiple Carriers** - Vietnam Post, GHN, GHTK, Viettel Post, J&T
- ✅ **Real-time Tracking** - Track shipment status and location
- ✅ **Automatic Status Updates** - Email notifications for all status changes
- ✅ **Regional Shipping Fees** - Dynamic pricing based on destination

### System Integration
- ✅ **RabbitMQ Message Queue** - Asynchronous event processing
- ✅ **Email Notifications** - Automated emails for all order events
- ✅ **API Retry Logic** - Polly retry policies for external API calls
- ✅ **Comprehensive Logging** - Serilog with transaction ID tracking

### Scalability & Performance
- ✅ **Load Balancing** - Nginx with least_conn strategy
- ✅ **Rate Limiting** - API and Web rate limiting (60/min, 1000/hour)
- ✅ **Kubernetes** - Zero-downtime rolling updates
- ✅ **Auto-scaling** - HPA based on CPU/Memory usage
- ✅ **Caching** - Static file caching for performance

### CI/CD & DevOps
- ✅ **GitHub Actions** - Automated testing and deployment
- ✅ **Jenkins Pipeline** - Alternative CI/CD with Jenkins
- ✅ **Docker** - Containerized applications
- ✅ **JMeter Load Testing** - Performance testing automation
- ✅ **Security Scanning** - Trivy and OWASP dependency check

## 📋 Architecture

```
┌─────────────┐      ┌──────────────┐      ┌─────────────┐
│   Browser   │─────▶│    Nginx     │─────▶│  Web (MVC)  │
└─────────────┘      │ Load Balancer│      └─────────────┘
                     │ Rate Limiting│             │
                     └──────────────┘             ▼
                            │              ┌─────────────┐
                            └─────────────▶│  API (REST) │
                                           └─────────────┘
                                                  │
                    ┌─────────────────────────────┼────────────────────┐
                    │                             │                    │
                    ▼                             ▼                    ▼
            ┌──────────────┐            ┌──────────────┐     ┌──────────────┐
            │   RabbitMQ   │            │    PayPal    │     │   Shipping   │
            │ Message Queue│            │   Sandbox    │     │   Provider   │
            └──────────────┘            └──────────────┘     └──────────────┘
```

## 🛠️ Technology Stack

### Backend
- **Framework**: ASP.NET Core 8.0 Web API
- **Message Queue**: RabbitMQ
- **Logging**: Serilog
- **Resilience**: Polly (Retry policies)
- **Payment**: PayPal SDK
- **Email**: System.Net.Mail (SMTP)

### Frontend
- **Framework**: ASP.NET Core 8.0 MVC
- **UI**: Bootstrap 5.3
- **Icons**: Font Awesome 6.4
- **Payment UI**: PayPal JavaScript SDK

### Infrastructure
- **Containerization**: Docker & Docker Compose
- **Orchestration**: Kubernetes
- **Load Balancer**: Nginx
- **CI/CD**: GitHub Actions, Jenkins
- **Testing**: JMeter (Performance), OWASP ZAP (Security)

## 📦 Project Structure

```
EbayClone/
├── EbayClone.API/                 # Backend API
│   ├── Controllers/               # API Controllers
│   ├── Services/                  # Business Logic
│   │   ├── PaymentService.cs     # PayPal integration
│   │   ├── ShippingService.cs    # Shipping management
│   │   ├── EmailService.cs       # Email notifications
│   │   ├── EscrowService.cs      # Escrow management
│   │   ├── OrderService.cs       # Order processing
│   │   └── RabbitMQService.cs    # Message queue
│   ├── Models/                    # Data models
│   ├── Middleware/                # Custom middleware
│   ├── Dockerfile                 # API container
│   └── appsettings.json          # Configuration
│
├── EbayClone.Web/                 # Frontend MVC
│   ├── Controllers/               # MVC Controllers
│   ├── Views/                     # Razor Views
│   │   ├── Home/
│   │   │   ├── Index.cshtml      # Product listing
│   │   │   ├── Product.cshtml    # Product detail
│   │   │   ├── OrderDetails.cshtml
│   │   │   ├── MyOrders.cshtml
│   │   │   └── PaymentApproval.cshtml
│   │   ├── Admin/
│   │   │   └── Orders.cshtml     # Order management
│   │   └── Shared/
│   │       └── _Layout.cshtml    # Main layout
│   ├── Models/                    # View models
│   ├── Dockerfile                 # Web container
│   └── appsettings.json
│
├── docker-compose.yml             # Multi-container setup
├── nginx.conf                     # Nginx configuration
├── k8s-deployment.yaml           # Kubernetes manifests
├── Jenkinsfile                   # Jenkins pipeline
├── .github/
│   └── workflows/
│       └── ci-cd.yml             # GitHub Actions
└── README.md                     # This file
```

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK
- Docker Desktop
- PayPal Sandbox Account
- (Optional) Kubernetes cluster
- (Optional) Jenkins server

### 1. Clone Repository

```bash
git clone https://github.com/yourusername/ebayclone.git
cd ebayclone
```

### 2. Configure PayPal Sandbox

1. Go to [PayPal Developer Dashboard](https://developer.paypal.com/dashboard/)
2. Create a Sandbox Business and Personal account
3. Get your **Client ID** and **Client Secret**
4. Update `appsettings.json`:

```json
{
  "PayPal": {
    "ClientId": "YOUR_CLIENT_ID_HERE",
    "ClientSecret": "YOUR_CLIENT_SECRET_HERE"
  }
}
```

5. Update `_Layout.cshtml` with your Client ID:

```html
<script src="https://www.paypal.com/sdk/js?client-id=YOUR_CLIENT_ID_HERE&currency=USD"></script>
```

### 3. Run with Docker Compose

```bash
# Build and start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

Services will be available at:
- **Web Frontend**: http://localhost:80
- **API**: http://localhost:7001
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

### 4. Run Locally (Development)

```bash
# Terminal 1 - Start RabbitMQ
docker run -d -p 5672:5672 -p 15672:15672 rabbitmq:3-management

# Terminal 2 - Run API
cd EbayClone.API
dotnet run

# Terminal 3 - Run Web
cd EbayClone.Web
dotnet run
```

Access:
- **Web**: https://localhost:5001
- **API**: https://localhost:7001

## 🧪 Testing

### Manual Testing Flow

1. **Browse Products**: Navigate to homepage
2. **Select Product**: Click "Buy Now" on any product
3. **Create Order**: Fill in shipping details, select region, apply coupon
4. **Pay with PayPal**:
   - Click "Pay with PayPal"
   - Login with Sandbox Personal account
   - Complete payment
5. **View Order**: Check order details and escrow status
6. **Admin Panel**: Go to `/Admin/Orders` to manage orders
7. **Ship Order**: Enter tracking number and carrier
8. **Update Status**: Mark as delivered
9. **Check Escrow**: After 7-21 days, funds released automatically

### Load Testing with JMeter

```bash
# Run load test
jmeter -n -t tests/load-test.jmx \
    -l results.jtl \
    -e -o report/ \
    -Jhostname=localhost \
    -Jthreads=100 \
    -Jrampup=60 \
    -Jduration=300

# View report
open report/index.html
```

### Security Testing with OWASP ZAP

```bash
docker run -t owasp/zap2docker-stable zap-baseline.py \
    -t http://localhost \
    -r zap-report.html
```

## 🐳 Docker Deployment

### Build Images

```bash
# Build API
docker build -t ebayclone/api:latest -f EbayClone.API/Dockerfile .

# Build Web
docker build -t ebayclone/web:latest -f EbayClone.Web/Dockerfile .
```

### Push to Registry

```bash
docker login
docker push ebayclone/api:latest
docker push ebayclone/web:latest
```

## ☸️ Kubernetes Deployment

### Deploy to Cluster

```bash
# Apply manifests
kubectl apply -f k8s-deployment.yaml

# Check status
kubectl get pods
kubectl get services
kubectl get hpa

# View logs
kubectl logs -f deployment/ebay-api
kubectl logs -f deployment/ebay-web

# Access application
kubectl port-forward service/ebay-web-service 8080:80
```

### Zero-Downtime Updates

```bash
# Update image
kubectl set image deployment/ebay-api \
    api=ebayclone/api:v2.0

# Monitor rollout
kubectl rollout status deployment/ebay-api

# Rollback if needed
kubectl rollout undo deployment/ebay-api
```

## 🔄 CI/CD Pipeline

### GitHub Actions

Automatically triggers on push to `main` or `develop`:

1. **Build & Test**: Compile code, run unit tests
2. **Security Scan**: Trivy vulnerability scanning
3. **Build Docker**: Create and push images
4. **Deploy Staging**: Auto-deploy to staging (develop branch)
5. **Performance Test**: JMeter load testing
6. **Deploy Production**: Manual approval (main branch)

### Jenkins Pipeline

```bash
# Trigger build
curl -X POST http://jenkins:8080/job/ebayclone/build \
    --user admin:token
```

Pipeline stages:
1. Checkout
2. Build
3. Unit Tests
4. Code Quality (SonarQube)
5. Security Scan
6. Build Docker Images
7. Deploy to Staging
8. Performance Testing
9. Deploy to Production (manual approval)
10. Smoke Tests

## 📊 Monitoring & Logging

### View Logs

```bash
# API logs
docker logs ebay-api -f

# RabbitMQ logs
docker logs ebay-rabbitmq -f

# Kubernetes logs
kubectl logs -f deployment/ebay-api
```

### Log Files

- API: `logs/ebayclone-{date}.txt`
- Transaction IDs tracked for all operations
- Email notifications logged (not sent in dev mode)

### Health Checks

- API Health: `http://localhost:7001/health`
- Web Health: `http://localhost:5001/`
- RabbitMQ: `http://localhost:15672`

## 🔐 Security Features

- **Rate Limiting**: 60 requests/minute per IP
- **JWT Authentication**: Token-based API security
- **HTTPS**: SSL/TLS encryption
- **Input Validation**: All inputs validated
- **CSRF Protection**: Anti-forgery tokens
- **Secure Headers**: X-Frame-Options, CSP, etc.
- **Dependency Scanning**: Automated vulnerability checks

## 📈 Performance Optimization

- **Caching**: Static files cached for 30 days
- **Compression**: Gzip compression enabled
- **Connection Pooling**: HTTP client reuse
- **Async Operations**: All I/O operations async
- **Load Balancing**: Nginx least_conn algorithm
- **Auto-scaling**: HPA maintains 3-10 pods based on load

## 🐛 Troubleshooting

### PayPal Sandbox Issues

**Problem**: "Payment processing failed"
**Solution**: 
1. Verify Client ID and Secret in appsettings.json
2. Check PayPal Sandbox account status
3. View API logs for detailed error messages

### RabbitMQ Connection Failed

**Problem**: "Failed to connect to RabbitMQ"
**Solution**:
1. Ensure RabbitMQ is running: `docker ps | grep rabbitmq`
2. Check connection settings in appsettings.json
3. Restart RabbitMQ: `docker restart ebay-rabbitmq`

### Order Not Shipping

**Problem**: Order stuck in "Paid" status
**Solution**:
1. Go to Admin panel `/Admin/Orders`
2. Click ship icon on the order
3. Enter tracking number and carrier
4. Submit to ship order

## 📝 API Documentation

### Key Endpoints

#### Orders
- `POST /api/orders` - Create order
- `GET /api/orders/{id}` - Get order details
- `GET /api/orders/user/{userId}` - Get user orders
- `POST /api/orders/{id}/payment/initiate` - Start payment
- `POST /api/orders/{id}/payment/complete` - Complete payment
- `POST /api/orders/{id}/ship` - Ship order
- `POST /api/orders/{id}/shipping/update` - Update shipping status
- `POST /api/orders/{id}/dispute` - Dispute order

### Request Examples

```bash
# Create Order
curl -X POST http://localhost:7001/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "buyer@example.com",
    "sellerId": "seller@example.com",
    "sellerRating": 4.5,
    "items": [{
      "productId": "1",
      "productName": "iPhone 15",
      "price": 1199.99,
      "quantity": 1
    }],
    "shippingAddress": "123 Main St, Hanoi",
    "shippingRegion": "Hanoi",
    "couponCode": "SAVE10"
  }'
```

## 🤝 Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open Pull Request

## 📄 License

This project is licensed under the MIT License.

## 👥 Team Management

Use Jira for:
- Sprint planning
- Task assignment
- Bug tracking
- Feature requests
- Release management

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/yourusername/ebayclone/issues)
- **Email**: support@ebayclone.com
- **Documentation**: [Wiki](https://github.com/yourusername/ebayclone/wiki)

## 🎯 Roadmap

- [ ] Add more payment methods (Stripe, Momo)
- [ ] Implement real-time chat support
- [ ] Add product reviews and ratings
- [ ] Mobile app (React Native)
- [ ] AI-powered product recommendations
- [ ] Multi-language support
- [ ] Advanced fraud detection

---

Made with ❤️ by eBay Clone Team