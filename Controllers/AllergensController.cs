using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AllergensController : ControllerBase
    {
        private readonly RestaurantContext _context;

        public AllergensController(RestaurantContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAllAllergens()
        {
            var allergens = await _context.ItemAllergens
                .Select(a => new { itemId = a.ItemId, allergenCode = a.AllergenCode })
                .ToListAsync();
            return Ok(allergens);
        }
    }
} 