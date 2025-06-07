using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;

namespace RestaurantApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly RestaurantContext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(RestaurantContext context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetHealth()
        {
            try
            {
                _logger.LogInformation("Health check requested");
                
                // Check database connection
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    _logger.LogWarning("Database connection failed");
                    return StatusCode(503, "Database connection failed");
                }

                // Check if we can query the database
                var categoryCount = await _context.Categories.CountAsync();
                var itemCount = await _context.Items.CountAsync();
                
                _logger.LogInformation("Health check successful - Categories: {CategoryCount}, Items: {ItemCount}", 
                    categoryCount, itemCount);

                return Ok(new
                {
                    Status = "Healthy",
                    Database = "Connected",
                    Categories = categoryCount,
                    Items = itemCount,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(500, new
                {
                    Status = "Unhealthy",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
} 