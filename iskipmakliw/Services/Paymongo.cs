using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace iskipmakliw.Services
{
    public class Paymongo : IPaymongo, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _secretKey;

        public Paymongo(IConfiguration config)
        {
            _secretKey = config["PayMongo:SecretKey"];
            _httpClient = new HttpClient();
            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_secretKey}:"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        }
        public async Task<string> CreateCheckoutSessionService(
                              decimal amount,
                              string currency,
                              string name,
                              string email,
                              string contact,
                              string productNames,
                              string paymentMethod)
        {
            var payload = new
            {
                data = new
                {
                    attributes = new
                    {
                        line_items = new[]
                        {
                new {
                    name = productNames,
                    amount = (int)(amount * 100), // in cents
                    currency = currency,
                    quantity = 1
                }
            },
                        payment_method_types = new[] { paymentMethod },
                        success_url = "https://localhost:7280/Payments/Success",
                        cancel_url = "https://localhost:7280/Payments/Cancel",
                        billing = new
                        {
                            name = name,
                            email = email,
                            phone = contact
                        },
                        metadata = new
                        {
                            customer_name = name,
                            customer_email = email,
                            customer_contact = contact
                        }
                    }
                }
            };
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.paymongo.com/v1/checkout_sessions", content);
            var responseString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error creating checkout session: {response.StatusCode} - {responseString}");
            }
            return responseString;
        }
        // Create Checkout Session
        public async Task<string> CreateCheckoutSession(
        decimal amount, // Can be removed or kept for validation
        string currency,
        string name,
        string email,
        string contact,
        List<(string name, double price, int quantity)> productDetails, // Changed parameter
        string paymentMethod)
            {
                // Create separate line items for each product
                var lineItems = productDetails.Select(product => new
                {
                    name = product.name,
                    amount = (int)(product.price * 100), // Convert to centavos
                    currency = currency,
                    quantity = product.quantity
                }).ToArray();

                var payload = new
                {
                    data = new
                    {
                        attributes = new
                        {
                            line_items = lineItems,
                            payment_method_types = new[] { paymentMethod },
                            success_url = "https://localhost:7280/Payments/SuccessPurchaseProduct",
                            cancel_url = "https://localhost:7280/Payments/Cancel",
                            billing = new
                            {
                                name = name,
                                email = email,
                                phone = contact
                            },
                            metadata = new
                            {
                                customer_name = name,
                                customer_email = email,
                                customer_contact = contact
                            }
                        }
                    }
                };

                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("https://api.paymongo.com/v1/checkout_sessions", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Error creating checkout session: {response.StatusCode} - {responseString}");
                }

                return responseString;
            }


        // Fetch checkout session details
        public async Task<string> GetCheckoutSession(string sessionId)
        {
            var response = await _httpClient.GetAsync($"https://api.paymongo.com/v1/checkout_sessions/{sessionId}");
            var responseString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error getting checkout session: {response.StatusCode} - {responseString}");
            }
            return responseString;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
