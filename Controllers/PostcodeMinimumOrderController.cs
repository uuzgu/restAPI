using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.Models;
using Microsoft.Extensions.Logging;

namespace RestaurantApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostcodeMinimumOrderController : ControllerBase
    {
        private readonly RestaurantContext _context;
        private readonly ILogger<PostcodeMinimumOrderController> _logger;

        public PostcodeMinimumOrderController(RestaurantContext context, ILogger<PostcodeMinimumOrderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("GetMinimumOrderValue/{postcode}")]
        public async Task<ActionResult<decimal>> GetMinimumOrderValue(string postcode)
        {
            _logger.LogInformation($"Getting minimum order value for postcode: {postcode}");
            
            var minimumOrder = await _context.PostcodeMinimumOrders
                .FirstOrDefaultAsync(p => p.Postcode == postcode);

            if (minimumOrder == null)
            {
                _logger.LogInformation($"No minimum order value found for postcode: {postcode}");
                return Ok(0); // Return 0 if no minimum order value is set for the postcode
            }

            _logger.LogInformation($"Found minimum order value: {minimumOrder.MinimumOrderValue} for postcode: {postcode}");
            return Ok(minimumOrder.MinimumOrderValue);
        }
    }
} 