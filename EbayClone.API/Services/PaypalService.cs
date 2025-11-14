using EbayClone.API.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EbayClone.API.Services
{
    public class AuthResponse
    {
        public string? scope { get; set; }
        public string? access_token { get; set; }
        public string? token_type { get; set; }
        public string? app_id { get; set; }
        public int? expires_in { get; set; }
        public string? nonce { get; set; }


    }
    public class ResponseOrder
    {
        public string? id { get; set; }
        public string? intent { get; set; }
        public string? status { get; set; }
        public List<PurchaseUnitResponse> purchase_units { get; set; } = new List<PurchaseUnitResponse>();
        public string? create_time { get; set; }
        public List<Links> links { get; set; } = new List<Links>();
    }
    public class Links
    {
        public string? href { get; set; }
        public string? rel { get; set; }
        public string? method { get; set; }
    }
    public class PurchaseUnitResponse
    {
        public string? reference_id { get; set; }
        public PurchaseUnitAmount amount { get; set; } = new PurchaseUnitAmount();
        public Payee payee { get; set; } = new Payee();
    }

    public class Payee
    {
        public string? email_address { get; set; }
        public string? merchant_id { get; set; }
    }
    public class CreatePaypalOrderRequest
    {
        public string intent { get; set; }
        public List<PurchaseUnit> purchase_units { get; set; }
    }

    public class PurchaseUnit
    {
        public PurchaseUnitAmount amount { get; set; }
    }

    public class PurchaseUnitAmount
    {
        public string currency_code { get; set; }
        public string value { get; set; }
    }
    public class PaypalService
    {
        private readonly HttpClient _http;
        IConfiguration _configuration;
        private readonly string _ppclienId;
        private readonly string _ppclientSecret;
        private readonly string Base = "https://api-m.sandbox.paypal.com";
        public PaypalService(HttpClient http, IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
            _ppclienId = _configuration["PayPal:ClientId"] ?? "";
            _ppclientSecret = _configuration["PayPal:ClientSecret"] ?? "";
        }

        private string ApiBase = "https://api-m.sandbox.paypal.com";

        public async Task<string> GetAccessTokenAsync()
        {
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_ppclienId}:{_ppclientSecret}"));
            var content = new List<KeyValuePair<string, string>>
            {
                new ("grant_type", "client_credentials")
            };
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{Base}/v1/oauth2/token"),
                Method = HttpMethod.Post,
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Basic", auth)
                }
                ,
                Content = new FormUrlEncodedContent(content)
            };
            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AuthResponse>(json).access_token ?? throw new Exception("No token");
        }

        public async Task<ResponseOrder> CreateOrderAsync(string value, string currency = "USD")
        {
            // Lấy access token
            var token = await GetAccessTokenAsync();

            // Tạo request body giống hệt Postman
            var body = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
            new
            {
                amount = new
                {
                    currency_code = currency,
                    value = value
                }
            }
        }
            };

            // Tạo HttpRequestMessage
            var req = new HttpRequestMessage(HttpMethod.Post, $"{Base}/v2/checkout/orders");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            req.Content = new StringContent(
                Newtonsoft.Json.JsonConvert.SerializeObject(body,
                    new Newtonsoft.Json.JsonSerializerSettings
                    {
                        ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                    }),
                Encoding.UTF8,
                "application/json"
            );
            Console.WriteLine("Request Body: " + await req.Content.ReadAsStringAsync());
            // Gửi request
            var res = await _http.SendAsync(req);
            var jsonResponse = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                throw new Exception($"Create order failed: {jsonResponse}");
            Console.WriteLine("Response Body: " + jsonResponse);
            // Deserialize response
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseOrder>(jsonResponse)
                   ?? throw new Exception("Deserialize order failed");
        }



        public async Task<JObject> CaptureOrderAsync(string paypalOrderId)
        {
            try
            {
                Console.WriteLine($"=== CAPTURING PAYPAL ORDER ===");
                Console.WriteLine($"PayPal Order ID: {paypalOrderId}");

                // Lấy access token
                var token = await GetAccessTokenAsync();
                Console.WriteLine($"Access Token obtained: {token.Substring(0, 20)}...");

                // Tạo request để capture order
                var req = new HttpRequestMessage(HttpMethod.Post, $"{Base}/v2/checkout/orders/{paypalOrderId}/capture");
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                req.Content = new StringContent("{}", Encoding.UTF8, "application/json");

                Console.WriteLine($"Request URL: {req.RequestUri}");

                // Gửi request
                var res = await _http.SendAsync(req);
                var jsonResponse = await res.Content.ReadAsStringAsync();

                Console.WriteLine($"Response Status: {res.StatusCode}");
                Console.WriteLine($"Response Body: {jsonResponse}");

                // Kiểm tra lỗi
                if (!res.IsSuccessStatusCode)
                {
                    throw new Exception($"PayPal Capture failed ({res.StatusCode}): {jsonResponse}");
                }

                // Parse response
                var captureResult = JObject.Parse(jsonResponse);

                // Log thông tin capture
                var status = captureResult["status"]?.ToString();
                var captureId = captureResult["purchase_units"]?[0]?["payments"]?["captures"]?[0]?["id"]?.ToString();

                Console.WriteLine($"Capture Status: {status}");
                Console.WriteLine($"Capture ID: {captureId}");
                Console.WriteLine("=== CAPTURE SUCCESSFUL ===");

                return captureResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== CAPTURE ERROR ===");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }
        public async Task<JObject> CreatePayoutAsync(string receiverEmail, decimal amount, string currency = "USD", string note = "")
        {
            var token = await GetAccessTokenAsync();
            var req = new HttpRequestMessage(HttpMethod.Post, $"{ApiBase}/v1/payments/payouts");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var batchId = Guid.NewGuid().ToString("N");
            var body = new
            {
                sender_batch_header = new
                {
                    sender_batch_id = batchId,
                    email_subject = "Bạn đã nhận được thanh toán từ EbayClone"
                },
                items = new[]
                {
                new {
                    recipient_type = "EMAIL",
                    amount = new { value = amount.ToString("0.00"), currency = currency },
                    receiver = receiverEmail,
                    note = note,
                    sender_item_id = Guid.NewGuid().ToString()
                }
            }
            };

            req.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            var res = await _http.SendAsync(req);
            var text = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode) throw new Exception($"Payout failed: {text}");
            return JObject.Parse(text);
        }
    }
}
