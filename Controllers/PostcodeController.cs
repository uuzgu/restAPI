using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostcodeController : ControllerBase
    {
        private readonly RestaurantContext _context;

        public PostcodeController(RestaurantContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Postcode>>> GetPostcodes()
        {
            return await _context.Postcodes
                .OrderBy(p => p.Code)
                .ToListAsync();
        }

        [HttpGet("{postcodeId}/addresses")]
        public async Task<ActionResult<IEnumerable<DeliveryAddress>>> GetAddressesByPostcode(int postcodeId)
        {
            var postcode = await _context.Postcodes.FindAsync(postcodeId);
            if (postcode == null)
            {
                return NotFound();
            }
            var addresses = await _context.DeliveryAddresses
                .Where(a => a.PostcodeId == postcodeId)
                .ToListAsync();
            return addresses;
        }

        // Debug endpoint to check database structure
        [HttpGet("debug")]
        public async Task<IActionResult> DebugDatabase()
        {
            try
            {
                // First, get all tables in the database
                var tables = await _context.Database.SqlQueryRaw<string>(
                    "SELECT name FROM sqlite_master WHERE type='table'"
                ).ToListAsync();

                // Then, for each table, get its structure
                var tableStructures = new Dictionary<string, object>();
                foreach (var table in tables)
                {
                    var columns = await _context.Database.SqlQueryRaw<string>(
                        $"PRAGMA table_info({table})"
                    ).ToListAsync();
                    tableStructures[table] = columns;
                }

                return Ok(new
                {
                    Tables = tables,
                    TableStructures = tableStructures
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message, StackTrace = ex.StackTrace });
            }
        }
    }
} 