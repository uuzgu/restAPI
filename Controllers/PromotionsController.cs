using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromotionsController : ControllerBase
    {
        private readonly RestaurantContext _context;
        private readonly ILogger<PromotionsController> _logger;

        public PromotionsController(RestaurantContext context, ILogger<PromotionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Promotion>>> GetActivePromotions()
        {
            try
            {
                _logger.LogInformation("GetActivePromotions called");
                var currentDate = DateTime.UtcNow;

                var activePromotions = await _context.Promotions
                    .Include(p => p.Item)
                    .Where(p => p.IsActive && 
                               p.StartDate <= currentDate && 
                               p.EndDate >= currentDate)
                    .ToListAsync();

                _logger.LogInformation($"Found {activePromotions.Count} active promotions");
                return activePromotions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active promotions");
                return StatusCode(500, "An error occurred while retrieving promotions");
            }
        }
    }
} 