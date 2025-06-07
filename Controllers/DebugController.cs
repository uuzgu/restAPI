using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.Models;
using System.Text.Json;

namespace RestaurantApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DebugController : ControllerBase
    {
        private readonly RestaurantContext _context;
        private readonly ILogger<DebugController> _logger;

        public DebugController(RestaurantContext context, ILogger<DebugController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("recent-orders")]
        public async Task<IActionResult> GetRecentOrders()
        {
            try
            {
                // Get recent orders with related data
                var recentOrders = await _context.Orders
                    .Include(o => o.CustomerInfo)
                    .Include(o => o.OrderDetails)
                    .OrderByDescending(o => o.Id)
                    .Take(5)
                    .ToListAsync();

                // Get recent order details
                var recentOrderDetails = await _context.OrderDetails
                    .OrderByDescending(od => od.Id)
                    .Take(5)
                    .ToListAsync();

                // Get recent customer info
                var recentCustomerInfo = await _context.CustomerOrderInfos
                    .OrderByDescending(c => c.Id)
                    .Take(5)
                    .ToListAsync();

                // Log the results
                _logger.LogInformation("Recent Orders: {Orders}", JsonSerializer.Serialize(recentOrders, new JsonSerializerOptions { WriteIndented = true }));
                _logger.LogInformation("Recent Order Details: {OrderDetails}", JsonSerializer.Serialize(recentOrderDetails, new JsonSerializerOptions { WriteIndented = true }));
                _logger.LogInformation("Recent Customer Info: {CustomerInfo}", JsonSerializer.Serialize(recentCustomerInfo, new JsonSerializerOptions { WriteIndented = true }));

                return Ok(new
                {
                    RecentOrders = recentOrders,
                    RecentOrderDetails = recentOrderDetails,
                    RecentCustomerInfo = recentCustomerInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recent orders");
                return StatusCode(500, new { Error = "An error occurred while fetching recent orders" });
            }
        }
    }
} 