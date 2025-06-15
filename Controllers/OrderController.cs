using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.Models;
using RestaurantApi.Services;
using Stripe;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Linq;

namespace RestaurantApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly RestaurantContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderController> _logger;
        private readonly IOrderService _orderService;

        public OrderController(RestaurantContext context, IConfiguration configuration, ILogger<OrderController> logger, IOrderService orderService)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                // Validate required fields based on order method
                if (request.OrderMethod.ToLower() == "delivery")
                {
                    if (string.IsNullOrEmpty(request.CustomerInfo?.PostalCode) ||
                        string.IsNullOrEmpty(request.CustomerInfo?.Street) ||
                        string.IsNullOrEmpty(request.CustomerInfo?.House))
                    {
                        return BadRequest(new { Error = "Postal code, street, and house number are required for delivery orders" });
                    }
                }

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
                        Comment = request.CustomerInfo.Comment,
                        CreateDate = DateTime.UtcNow
                    };

                    // Create delivery address if needed
                    if (request.OrderMethod.ToLower() == "delivery" && !string.IsNullOrEmpty(request.CustomerInfo.PostalCode))
                    {
                        var postcode = await _context.Postcodes
                            .FirstOrDefaultAsync(p => p.Code == request.CustomerInfo.PostalCode);

                        if (postcode == null)
                        {
                            return BadRequest(new { Error = "Invalid postal code" });
                        }

                        var deliveryAddress = new DeliveryAddress
                        {
                            PostcodeId = postcode.Id,
                            Street = request.CustomerInfo.Street ?? string.Empty,
                            House = request.CustomerInfo.House,
                            Stairs = request.CustomerInfo.Stairs,
                            Stick = request.CustomerInfo.Stick,
                            Door = request.CustomerInfo.Door,
                            Bell = request.CustomerInfo.Bell
                        };
                        _context.DeliveryAddresses.Add(deliveryAddress);
                        await _context.SaveChangesAsync();
                    }
                }

                // Check for required options
                foreach (var item in request.Items)
                {
                    var itemWithOptions = await _context.Items
                        .Include(i => i.ItemSelectionGroups)
                            .ThenInclude(isg => isg.SelectionGroup)
                                .ThenInclude(sg => sg.SelectionOptions)
                        .FirstOrDefaultAsync(i => i.Id == item.Id);

                    if (itemWithOptions != null)
                    {
                        foreach (var selectionGroup in itemWithOptions.ItemSelectionGroups.Select(isg => isg.SelectionGroup))
                        {
                            if (selectionGroup.IsRequired && !selectionGroup.SelectionOptions.Any(o => item.SelectedOptions.Contains(o.Id)))
                            {
                                return BadRequest(new { Error = $"Required option not selected for {itemWithOptions.Name} in {selectionGroup.Name}" });
                            }
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

                // Log recent orders after creation
                var recentOrders = await _context.Orders
                    .Include(o => o.CustomerInfo)
                    .Include(o => o.OrderDetails)
                    .OrderByDescending(o => o.Id)
                    .Take(5)
                    .ToListAsync();

                _logger.LogInformation("Recent Orders after creation: {Orders}", 
                    JsonSerializer.Serialize(recentOrders, new JsonSerializerOptions { WriteIndented = true }));

                return Ok(new { OrderId = order.Id, OrderNumber = order.OrderNumber });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return BadRequest(new { Error = ex.Message });
            }
        } 

        [HttpPost("create-cash-order")]
        public async Task<IActionResult> CreateCashOrder([FromBody] CreateCashOrderRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Received cash order request - Total Amount: {TotalAmount}, Order Method: {OrderMethod}, Payment Method: {PaymentMethod}", 
                    request.TotalAmount, request.OrderMethod, request.PaymentMethod);
                
                if (request.Items == null || !request.Items.Any())
                {
                    return BadRequest("No items in the order");
                }

                // Validate delivery information if it's a delivery order
                if (request.OrderMethod.ToLower() == "delivery")
                {
                    if (string.IsNullOrEmpty(request.CustomerInfo.PostalCode))
                    {
                        return BadRequest(new { Error = "Postal code is required for delivery orders" });
                    }
                    if (string.IsNullOrEmpty(request.CustomerInfo.Street))
                    {
                        return BadRequest(new { Error = "Street address is required for delivery orders" });
                    }
                    if (string.IsNullOrEmpty(request.CustomerInfo.House))
                    {
                        return BadRequest(new { Error = "House number is required for delivery orders" });
                    }
                }

                // Create customer info
                var customerInfo = new CustomerOrderInfo
                {
                    FirstName = request.CustomerInfo.FirstName,
                    LastName = request.CustomerInfo.LastName,
                    Email = request.CustomerInfo.Email,
                    Phone = request.CustomerInfo.Phone,
                    Comment = request.CustomerInfo.Comment,
                    CreateDate = DateTime.UtcNow
                };
                _context.CustomerOrderInfos.Add(customerInfo);
                await _context.SaveChangesAsync();

                // Create delivery address if needed
                DeliveryAddress? deliveryAddress = null;
                if (request.OrderMethod.ToLower() == "delivery" && !string.IsNullOrEmpty(request.CustomerInfo.PostalCode))
                {
                    var postcode = await _context.Postcodes
                        .FirstOrDefaultAsync(p => p.Code == request.CustomerInfo.PostalCode);

                    if (postcode == null)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { Error = "Invalid postal code" });
                    }

                    deliveryAddress = new DeliveryAddress
                    {
                        PostcodeId = postcode.Id,
                        Street = request.CustomerInfo.Street ?? string.Empty,
                        House = request.CustomerInfo.House,
                        Stairs = request.CustomerInfo.Stairs,
                        Stick = request.CustomerInfo.Stick,
                        Door = request.CustomerInfo.Door,
                        Bell = request.CustomerInfo.Bell
                    };
                    _context.DeliveryAddresses.Add(deliveryAddress);
                    await _context.SaveChangesAsync();
                }

                // Create the order
                var order = new Order
                {
                    OrderNumber = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                    Status = request.Status,
                    Total = request.TotalAmount,
                    PaymentMethod = request.PaymentMethod,
                    OrderMethod = request.OrderMethod,
                    SpecialNotes = request.SpecialNotes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CustomerInfo = customerInfo,
                    DeliveryAddress = deliveryAddress
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Create order details
                var itemsResponse = new List<OrderItemResponse>();
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

                    // Add to response
                    itemsResponse.Add(new OrderItemResponse
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Price = item.Price,
                        Quantity = item.Quantity,
                        Note = item.Notes,
                        SelectedItems = selectedItems,
                        GroupOrder = item.GroupOrder ?? new List<string>(),
                        Image = item.Image,
                        OriginalPrice = item.OriginalPrice
                    });
                }
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // Calculate original total (base original price only, since options are included in the price)
                decimal originalTotal = 0;
                foreach (var item in request.Items)
                {
                    originalTotal += item.OriginalPrice;
                }

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

                    _logger.LogInformation("Recent Orders after cash order creation: {Orders}", 
                        JsonSerializer.Serialize(recentOrders, new JsonSerializerOptions { WriteIndented = true }));
                }
                catch (Exception logEx)
                {
                    _logger.LogWarning(logEx, "Error logging recent orders");
                }

                _logger.LogInformation("Successfully created cash order with ID: {OrderId}", order.Id);

                return Ok(new
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    Status = order.Status,
                    Total = order.Total,
                    PaymentMethod = order.PaymentMethod,
                    OrderMethod = order.OrderMethod,
                    CreatedAt = order.CreatedAt,
                    DiscountCoupon = request.DiscountCoupon,
                    CustomerInfo = new
                    {
                        FirstName = order.CustomerInfo.FirstName,
                        LastName = order.CustomerInfo.LastName,
                        Email = order.CustomerInfo.Email,
                        Phone = order.CustomerInfo.Phone,
                        PostalCode = order.DeliveryAddress?.Postcode?.Code,
                        Street = order.DeliveryAddress?.Street,
                        House = order.DeliveryAddress?.House,
                        Stairs = order.DeliveryAddress?.Stairs,
                        Stick = order.DeliveryAddress?.Stick,
                        Door = order.DeliveryAddress?.Door,
                        Bell = order.DeliveryAddress?.Bell,
                        SpecialNotes = order.SpecialNotes
                    },
                    SpecialNotes = order.SpecialNotes,
                    Items = itemsResponse,
                    OriginalTotal = originalTotal
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating cash order");
                
                // Log inner exception if it exists
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner exception: {InnerException}", ex.InnerException.Message);
                    _logger.LogError("Inner exception stack trace: {StackTrace}", ex.InnerException.StackTrace);
                }
                
                return StatusCode(500, new { 
                    Error = "An error occurred while creating the order", 
                    Details = ex.Message,
                    InnerException = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.CustomerInfo)
                    .Include(o => o.DeliveryAddress)
                        .ThenInclude(d => d.Postcode)
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return NotFound(new { Error = "Order not found" });
                }

                // Get order items with safe deserialization
                var items = new List<OrderItemResponse>();
                foreach (var od in order.OrderDetails)
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

                        items.Add(new OrderItemResponse
                        {
                            Id = itemDetails.TryGetValue("id", out var idElem) ? idElem.GetInt32() : 0,
                            Name = itemDetails.TryGetValue("name", out var nameElem) ? nameElem.GetString() ?? "" : "",
                            Price = itemDetails.TryGetValue("price", out var priceElem) ? priceElem.GetDecimal() : 0,
                            Quantity = itemDetails.TryGetValue("quantity", out var qtyElem) ? qtyElem.GetInt32() : 1,
                            Note = itemDetails.TryGetValue("note", out var noteElem) ? noteElem.GetString() ?? "" : "",
                            SelectedItems = selectedItems,
                            GroupOrder = groupOrder,
                            Image = itemDetails.TryGetValue("image", out var imgElem) ? imgElem.GetString() ?? "" : "",
                            OriginalPrice = itemDetails.TryGetValue("originalPrice", out var originalPriceElem) ? originalPriceElem.GetDecimal() : 0
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
                    originalTotal += item.OriginalPrice;
                }

                // Map customer info to match create-cash-order response format
                var customerInfo = order.CustomerInfo != null ? new {
                    FirstName = order.CustomerInfo.FirstName,
                    LastName = order.CustomerInfo.LastName,
                    Email = order.CustomerInfo.Email,
                    Phone = order.CustomerInfo.Phone,
                    PostalCode = order.DeliveryAddress?.Postcode?.Code,
                    Street = order.DeliveryAddress?.Street,
                    House = order.DeliveryAddress?.House,
                    Stairs = order.DeliveryAddress?.Stairs,
                    Stick = order.DeliveryAddress?.Stick,
                    Door = order.DeliveryAddress?.Door,
                    Bell = order.DeliveryAddress?.Bell,
                    SpecialNotes = order.SpecialNotes
                } : null;

                var response = new OrderResponse
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    Status = order.Status,
                    Total = order.Total,
                    PaymentMethod = order.PaymentMethod,
                    OrderMethod = order.OrderMethod,
                    CreatedAt = order.CreatedAt,
                    CustomerInfo = customerInfo,
                    Items = items,
                    DiscountCoupon = items.Any(item => item.Price < item.OriginalPrice) ? 1 : 0,
                    SpecialNotes = order.SpecialNotes,
                    OriginalTotal = originalTotal
                };

                _logger.LogInformation("Returning order details: {OrderDetails}", 
                    JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order {OrderId}", id);
                _logger.LogError("Error details: {ErrorDetails}", ex.ToString());
                return StatusCode(500, new { Error = "An error occurred while fetching the order", Details = ex.Message });
            }
        }
    }

    public class CreateOrderRequest
    {
        [Required]
        public List<OrderItemRequest> Items { get; set; } = new();
        [Required]
        public CustomerInfoRequest CustomerInfo { get; set; } = new();
        [Required]
        public string OrderMethod { get; set; } = string.Empty;
        [Required]
        public string PaymentMethod { get; set; } = string.Empty;
        [JsonPropertyName("specialNotes")]
        public string? SpecialNotes { get; set; }
        [Required]
        public string Status { get; set; } = string.Empty;
        [Required]
        public decimal TotalAmount { get; set; }
    }

    public class CreateCashOrderRequest
    {
        [Required]
        public List<OrderItemRequest> Items { get; set; } = new();
        [Required]
        public CustomerInfoRequest CustomerInfo { get; set; } = new();
        [JsonPropertyName("specialNotes")]
        public string? SpecialNotes { get; set; }
        [Required]
        public string OrderMethod { get; set; } = string.Empty;
        [Required]
        public string PaymentMethod { get; set; } = string.Empty;
        [Required]
        public string Status { get; set; } = string.Empty;
        [Required]
        public decimal TotalAmount { get; set; }
        public decimal DiscountCoupon { get; set; }
    }

    public class CustomerInfoRequest
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

        public string? Comment { get; set; }
    }

    public class OrderItemRequest
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public decimal OriginalPrice { get; set; }

        public string? Notes { get; set; }

        public List<int> SelectedOptions { get; set; } = new();

        public List<SelectedItemRequest>? SelectedItems { get; set; }

        public List<string>? GroupOrder { get; set; }

        public string? Image { get; set; }
    }

    public class SelectedItemRequest
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public string? GroupName { get; set; }

        public string? Type { get; set; }
    }

    public class OrderItemResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? Note { get; set; }
        public List<object> SelectedItems { get; set; } = new();
        public List<string> GroupOrder { get; set; } = new();
        public string? Image { get; set; }
        public decimal OriginalPrice { get; set; }
    }

    public class OrderResponse
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string OrderMethod { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public object? CustomerInfo { get; set; }
        public List<OrderItemResponse> Items { get; set; } = new();
        public int DiscountCoupon { get; set; }
        public string? SpecialNotes { get; set; }
        public decimal OriginalTotal { get; set; }
    }
} 