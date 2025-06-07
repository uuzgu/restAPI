using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.Models;
using Stripe;
using Stripe.Checkout;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RestaurantApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StripeController : ControllerBase
    {
        private readonly RestaurantContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeController> _logger;

        public StripeController(RestaurantContext context, IConfiguration configuration, ILogger<StripeController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
        {
            try
            {
                var stripeApiKey = _configuration["Stripe:SecretKey"];
                if (string.IsNullOrEmpty(stripeApiKey))
                {
                    return StatusCode(500, "Stripe API key is not configured");
                }

                StripeConfiguration.ApiKey = stripeApiKey;

                _logger.LogInformation("Creating checkout session - Total Amount: {TotalAmount}, Order Method: {OrderMethod}, Payment Method: {PaymentMethod}", 
                    request.TotalAmount, request.OrderMethod, request.PaymentMethod);

                // Create a new order
                var order = new Order
                {
                    OrderNumber = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                    Status = request.Status,
                    Total = request.TotalAmount,
                    PaymentMethod = request.PaymentMethod,
                    OrderMethod = request.OrderMethod,
                    SpecialNotes = request.SpecialNotes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Add customer info if provided
                if (request.CustomerInfo != null)
                {
                    order.CustomerInfo = new CustomerOrderInfo
                    {
                        FirstName = request.CustomerInfo.FirstName,
                        LastName = request.CustomerInfo.LastName,
                        Email = request.CustomerInfo.Email,
                        Phone = request.CustomerInfo.Phone,
                        CreateDate = DateTime.UtcNow
                    };

                    // Add delivery address if it's a delivery order
                    if (request.OrderMethod.ToLower() == "delivery" && !string.IsNullOrEmpty(request.CustomerInfo.PostalCode))
                    {
                        var postcode = await _context.Postcodes
                            .FirstOrDefaultAsync(p => p.Code == request.CustomerInfo.PostalCode);

                        if (postcode != null)
                        {
                            order.DeliveryAddress = new DeliveryAddress
                            {
                                PostcodeId = postcode.Id,
                                Street = request.CustomerInfo.Street ?? string.Empty,
                                House = request.CustomerInfo.House,
                                Stairs = request.CustomerInfo.Stairs,
                                Stick = request.CustomerInfo.Stick,
                                Door = request.CustomerInfo.Door,
                                Bell = request.CustomerInfo.Bell
                            };
                        }
                    }
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Create order details
                foreach (var item in request.Items)
                {
                    var selectedItems = (item.SelectedItems != null)
                        ? item.SelectedItems.Select(si => new
                        {
                            id = si.Id,
                            name = si.Name,
                            groupName = si.GroupName,
                            type = si.Type,
                            price = si.Price,
                            quantity = si.Quantity
                        }).Cast<object>().ToList()
                        : new List<object>();

                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ItemDetails = JsonSerializer.Serialize(new
                        {
                            id = item.Id,
                            name = item.Name,
                            quantity = item.Quantity,
                            price = item.Price,
                            note = item.Notes,
                            selectedItems = selectedItems,
                            groupOrder = item.GroupOrder ?? new List<string>(),
                            image = item.Image
                        }, new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        })
                    };

                    _context.OrderDetails.Add(orderDetail);
                }
                await _context.SaveChangesAsync();

                // Log recent orders after creation
                try
                {
                    var recentOrders = await _context.Orders
                        .Include(o => o.CustomerInfo)
                        .Include(o => o.OrderDetails)
                        .OrderByDescending(o => o.Id)
                        .Take(5)
                        .Select(o => new
                        {
                            o.Id,
                            o.OrderNumber,
                            o.Status,
                            o.Total,
                            o.PaymentMethod,
                            o.OrderMethod,
                            CustomerInfo = o.CustomerInfo != null ? new
                            {
                                o.CustomerInfo.FirstName,
                                o.CustomerInfo.LastName,
                                o.CustomerInfo.Email
                            } : null,
                            OrderDetailsCount = o.OrderDetails.Count
                        })
                        .ToListAsync();

                    _logger.LogInformation("Recent Orders after Stripe checkout creation: {Orders}", 
                        JsonSerializer.Serialize(recentOrders, new JsonSerializerOptions { WriteIndented = true }));
                }
                catch (Exception logEx)
                {
                    _logger.LogWarning(logEx, "Error logging recent orders");
                }

                var frontendUrl = _configuration["FrontendUrl"];
                if (string.IsNullOrEmpty(frontendUrl))
                {
                    // Try to get the frontend URL from the request origin
                    var origin = Request.Headers["Origin"].ToString();
                    if (!string.IsNullOrEmpty(origin))
                    {
                        frontendUrl = origin;
                    }
                    else
                    {
                        return StatusCode(500, "Frontend URL is not configured and could not be determined from request");
                    }
                }

                _logger.LogInformation("Using frontend URL for redirects: {FrontendUrl}", frontendUrl);

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = request.Items.Select(item => new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "eur",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Name,
                            },
                            UnitAmount = (long)(item.Price * 100), // Convert to cents
                        },
                        Quantity = item.Quantity,
                    }).ToList(),
                    Mode = "payment",
                    SuccessUrl = $"{frontendUrl}/payment/success?session_id={{CHECKOUT_SESSION_ID}}&payment_method=stripe",
                    CancelUrl = $"{frontendUrl}/payment/cancel?session_id={{CHECKOUT_SESSION_ID}}&payment_method=stripe",
                    CustomerEmail = request.CustomerInfo?.Email,
                    Metadata = new Dictionary<string, string>
                    {
                        { "orderId", order.Id.ToString() },
                        { "orderNumber", order.OrderNumber }
                    }
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                return Ok(new { sessionId = session.Id, url = session.Url });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating checkout session");
                return StatusCode(500, $"Error creating checkout session: {ex.Message}");
            }
        }

        [HttpGet("payment-success")]
        public async Task<IActionResult> PaymentSuccess([FromQuery] string session_id)
        {
            try
            {
                var service = new SessionService();
                var session = await service.GetAsync(session_id);

                if (session == null)
                {
                    return NotFound("Session not found");
                }

                if (!session.Metadata.TryGetValue("orderId", out string? orderIdStr) || 
                    !int.TryParse(orderIdStr, out int orderId))
                {
                    return BadRequest("Invalid order ID in session metadata");
                }

                // Update order status in database
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return NotFound("Order not found");
                }

                // Update order status if payment is successful
                if (session.PaymentStatus == "paid")
                {
                    order.Status = "completed";
                    order.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    orderId = order.Id,
                    orderNumber = order.OrderNumber,
                    total = order.Total,
                    status = order.Status,
                    paymentMethod = order.PaymentMethod
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment success");
                return StatusCode(500, "An error occurred while processing the payment");
            }
        }

        [HttpGet("payment-cancel")]
        [HttpPost("payment-cancel")]
        public async Task<ActionResult> HandlePaymentCancel([FromQuery] string session_id, [FromBody] PaymentSuccessRequest? request = null)
        {
            try
            {
                var sessionId = session_id ?? request?.SessionId;
                if (string.IsNullOrEmpty(sessionId))
                {
                    return BadRequest("Session ID is required");
                }

                var service = new SessionService();
                var session = await service.GetAsync(sessionId);

                if (session == null)
                {
                    return NotFound("Session not found");
                }

                if (!session.Metadata.TryGetValue("orderId", out string? orderIdStr) || 
                    !int.TryParse(orderIdStr, out int orderId))
                {
                    return BadRequest("Invalid order ID in session metadata");
                }

                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return NotFound("Order not found");
                }

                order.Status = "cancelled";
                order.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    orderId = order.Id,
                    orderNumber = order.OrderNumber,
                    status = order.Status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment cancellation");
                return StatusCode(500, "An error occurred while processing the payment cancellation");
            }
        }

        [HttpPost("create-payment-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId);

                if (order == null)
                {
                    return NotFound("Order not found");
                }

                var stripeApiKey = _configuration["Stripe:SecretKey"];
                if (string.IsNullOrEmpty(stripeApiKey))
                {
                    return StatusCode(500, "Stripe API key is not configured");
                }

                StripeConfiguration.ApiKey = stripeApiKey;

                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(order.Total * 100), // Convert to cents
                    Currency = "eur",
                    PaymentMethodTypes = new List<string> { "card" },
                    Metadata = new Dictionary<string, string>
                    {
                        { "orderId", order.Id.ToString() },
                        { "orderNumber", order.OrderNumber }
                    }
                };

                var service = new PaymentIntentService();
                var intent = await service.CreateAsync(options);

                return Ok(new { clientSecret = intent.ClientSecret });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment intent");
                return StatusCode(500, $"Error creating payment intent: {ex.Message}");
            }
        }
    }

    public class PaymentSuccessRequest
    {
        [Required]
        public string SessionId { get; set; } = string.Empty;
    }

    public class CreateCheckoutSessionRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = string.Empty;

        [Required]
        public string OrderMethod { get; set; } = string.Empty;

        [JsonPropertyName("specialNotes")]
        public string? SpecialNotes { get; set; }

        [Required]
        public List<StripeCheckoutItem> Items { get; set; } = new();

        public CustomerInfo? CustomerInfo { get; set; }
    }

    public class CreatePaymentIntentRequest
    {
        public int OrderId { get; set; }
    }

    public class CustomerInfo
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string? PostalCode { get; set; }

        public string? Street { get; set; }

        public string? House { get; set; }

        public string? Stairs { get; set; }

        public string? Stick { get; set; }

        public string? Door { get; set; }

        public string? Bell { get; set; }
    }
}
