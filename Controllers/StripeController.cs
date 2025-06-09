using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.Models;
using RestaurantApi.Services;
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
        private readonly IOrderService _orderService;

        public StripeController(RestaurantContext context, IConfiguration configuration, ILogger<StripeController> logger, IOrderService orderService)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _orderService = orderService;
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
        {
            try
            {
                var stripeApiKey = _configuration["Stripe:SecretKey"];
                _logger.LogInformation("Stripe API Key from configuration: {StripeApiKey}", stripeApiKey);
                if (string.IsNullOrEmpty(stripeApiKey))
                {
                    _logger.LogError("Stripe API key is not configured");
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
                            originalPrice = item.OriginalPrice,
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

                // Calculate original total (base original price only, since options are included in the price)
                decimal originalTotal = 0;
                foreach (var item in request.Items)
                {
                    originalTotal += item.OriginalPrice * item.Quantity;
                }

                // Check if any item has a discount applied
                bool hasDiscount = request.Items.Any(item => item.Price < item.OriginalPrice);

                // Store discount information in metadata
                var metadata = new Dictionary<string, string>
                {
                    { "orderId", order.Id.ToString() },
                    { "orderNumber", order.OrderNumber },
                    { "hasDiscount", hasDiscount ? "1" : "0" },
                    { "originalTotal", originalTotal.ToString() }
                };

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
                                Description = item.SelectedItems?.Any() == true 
                                    ? string.Join(", ", item.SelectedItems.Select(si => $"{si.Name} (+{si.Price:C})"))
                                    : null
                            },
                            UnitAmount = (long)(item.Price * 100), // Convert to cents
                        },
                        Quantity = item.Quantity,
                    }).ToList(),
                    Mode = "payment",
                    SuccessUrl = $"{frontendUrl}/payment/success?session_id={{CHECKOUT_SESSION_ID}}&payment_method=stripe",
                    CancelUrl = $"{frontendUrl}/payment/cancel?session_id={{CHECKOUT_SESSION_ID}}&payment_method=stripe",
                    CustomerEmail = request.CustomerInfo?.Email,
                    Metadata = metadata
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

                // Get complete order details using OrderService
                var completeOrder = await _orderService.GetOrderById(orderId);
                if (completeOrder == null)
                {
                    return NotFound("Order not found");
                }

                // Get order items with safe deserialization
                var items = new List<OrderItemResponse>();
                foreach (var od in completeOrder.OrderDetails)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(od.ItemDetails))
                        {
                            _logger.LogWarning("Empty ItemDetails for OrderDetail {OrderDetailId}", od.Id);
                            continue;
                        }

                        var itemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(od.ItemDetails);
                        if (itemDetails == null)
                        {
                            _logger.LogWarning("Failed to deserialize ItemDetails for OrderDetail {OrderDetailId}", od.Id);
                            continue;
                        }

                        // Extract selectedItems
                        var selectedItems = new List<object>();
                        if (itemDetails.TryGetValue("selectedItems", out var siElem) && 
                            siElem.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var si in siElem.EnumerateArray())
                            {
                                if (si.ValueKind == JsonValueKind.Object)
                                {
                                    var siDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(si.GetRawText());
                                    if (siDict != null)
                                    {
                                        selectedItems.Add(new
                                        {
                                            id = siDict.TryGetValue("id", out var siIdElem) ? siIdElem.GetInt32() : 0,
                                            name = siDict.TryGetValue("name", out var siNameElem) ? siNameElem.GetString() ?? "" : "",
                                            price = siDict.TryGetValue("price", out var siPriceElem) ? siPriceElem.GetDecimal() : 0,
                                            quantity = siDict.TryGetValue("quantity", out var siQtyElem) ? siQtyElem.GetInt32() : 1,
                                            groupName = siDict.TryGetValue("groupName", out var siGroupElem) ? siGroupElem.GetString() : null,
                                            type = siDict.TryGetValue("type", out var siTypeElem) ? siTypeElem.GetString() : null
                                        });
                                    }
                                }
                            }
                        }

                        // Extract groupOrder
                        var groupOrder = new List<string>();
                        if (itemDetails.TryGetValue("groupOrder", out var goElem) && 
                            goElem.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var go in goElem.EnumerateArray())
                            {
                                if (go.ValueKind == JsonValueKind.String)
                                {
                                    groupOrder.Add(go.GetString() ?? "");
                                }
                            }
                        }

                        var price = itemDetails.TryGetValue("price", out var priceElem) ? priceElem.GetDecimal() : 0;
                        var originalPrice = itemDetails.TryGetValue("originalPrice", out var originalPriceElem) ? originalPriceElem.GetDecimal() : price;

                        items.Add(new OrderItemResponse
                        {
                            Id = itemDetails.TryGetValue("id", out var idElem) ? idElem.GetInt32() : 0,
                            Name = itemDetails.TryGetValue("name", out var nameElem) ? nameElem.GetString() ?? "" : "",
                            Price = price,
                            Quantity = itemDetails.TryGetValue("quantity", out var qtyElem) ? qtyElem.GetInt32() : 1,
                            Note = itemDetails.TryGetValue("note", out var noteElem) ? noteElem.GetString() ?? "" : "",
                            SelectedItems = selectedItems,
                            GroupOrder = groupOrder,
                            Image = itemDetails.TryGetValue("image", out var imgElem) ? imgElem.GetString() ?? "" : "",
                            OriginalPrice = originalPrice
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deserializing order detail {OrderDetailId}", od.Id);
                        continue;
                    }
                }

                // Calculate original total (base original price only, since options are included in the price)
                decimal originalTotal = 0;
                foreach (var item in items)
                {
                    originalTotal += item.OriginalPrice * item.Quantity;
                }

                // Map customer info to match create-cash-order response format
                var customerInfo = completeOrder.CustomerInfo != null ? new {
                    FirstName = completeOrder.CustomerInfo.FirstName,
                    LastName = completeOrder.CustomerInfo.LastName,
                    Email = completeOrder.CustomerInfo.Email,
                    Phone = completeOrder.CustomerInfo.Phone,
                    PostalCode = completeOrder.DeliveryAddress?.PostcodeId,
                    Street = completeOrder.DeliveryAddress?.Street,
                    House = completeOrder.DeliveryAddress?.House,
                    Stairs = completeOrder.DeliveryAddress?.Stairs,
                    Stick = completeOrder.DeliveryAddress?.Stick,
                    Door = completeOrder.DeliveryAddress?.Door,
                    Bell = completeOrder.DeliveryAddress?.Bell,
                    SpecialNotes = completeOrder.SpecialNotes
                } : null;

                var response = new OrderResponse
                {
                    OrderId = completeOrder.Id,
                    OrderNumber = completeOrder.OrderNumber,
                    Status = completeOrder.Status,
                    Total = completeOrder.Total,
                    PaymentMethod = completeOrder.PaymentMethod,
                    OrderMethod = completeOrder.OrderMethod,
                    CreatedAt = completeOrder.CreatedAt,
                    CustomerInfo = customerInfo,
                    Items = items,
                    DiscountCoupon = items.Any(item => item.Price < item.OriginalPrice) ? 1 : 0,
                    SpecialNotes = completeOrder.SpecialNotes,
                    OriginalTotal = originalTotal
                };

                _logger.LogInformation("Returning order details: {OrderDetails}", 
                    JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));

                return Ok(response);
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

    public class StripeCheckoutItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }
        public string? Image { get; set; }
        public List<string>? GroupOrder { get; set; }
        public List<SelectedItem>? SelectedItems { get; set; }
    }
}
