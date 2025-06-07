using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.Models;

namespace RestaurantApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly RestaurantContext _context;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(RestaurantContext context, ILogger<CategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            try
            {
                _logger.LogInformation("Fetching all categories");
                var categories = await _context.Categories
                    .Include(c => c.Items)
                    .Include(c => c.CategorySelectionGroups)
                        .ThenInclude(csg => csg.SelectionGroup)
                            .ThenInclude(sg => sg.SelectionOptions)
                    .ToListAsync();
                
                // Define the desired order of categories
                var orderedCategories = categories.OrderBy(c => c.Id switch
                {
                    0 => 1,  // Promotions
                    1 => 2,  // Pizza
                    2 => 3,  // Bowl
                    3 => 4,  // Cheeseburger
                    4 => 5,  // Salad
                    5 => 6,  // Breakfast
                    6 => 7,  // Drinks
                    7 => 8,  // Soup
                    8 => 9,  // Dessert
                    _ => 10  // Any other categories
                });
                
                _logger.LogInformation("Found {Count} categories", categories.Count);
                return Ok(orderedCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories");
                return StatusCode(500, "An error occurred while fetching categories");
            }
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            try
            {
                _logger.LogInformation("Fetching category with ID: {Id}", id);
                var category = await _context.Categories
                    .Include(c => c.Items)
                    .Include(c => c.CategorySelectionGroups)
                        .ThenInclude(csg => csg.SelectionGroup)
                            .ThenInclude(sg => sg.SelectionOptions)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    _logger.LogWarning("Category with ID {Id} not found", id);
                    return NotFound();
                }

                _logger.LogInformation("Found category: {Category}", category.Name);
                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching category with ID: {Id}", id);
                return StatusCode(500, "An error occurred while fetching the category");
            }
        }

        // POST: api/Categories
        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory(Category category)
        {
            try
            {
                if (string.IsNullOrEmpty(category.Name))
                {
                    return BadRequest("Category name is required");
                }

                _logger.LogInformation("Creating new category: {Name}", category.Name);
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created category with ID: {Id}", category.Id);
                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, "An error occurred while creating the category");
            }
        }

        // PUT: api/Categories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, Category category)
        {
            try
            {
                if (id != category.Id)
                {
                    return BadRequest();
                }

                if (string.IsNullOrEmpty(category.Name))
                {
                    return BadRequest("Category name is required");
                }

                _logger.LogInformation("Updating category with ID: {Id}", id);
                _context.Entry(category).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(id))
                    {
                        _logger.LogWarning("Category with ID {Id} not found", id);
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                _logger.LogInformation("Updated category with ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category with ID: {Id}", id);
                return StatusCode(500, "An error occurred while updating the category");
            }
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                _logger.LogInformation("Deleting category with ID: {Id}", id);
                var category = await _context.Categories
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    _logger.LogWarning("Category with ID {Id} not found", id);
                    return NotFound();
                }

                if (category.Items.Any())
                {
                    return BadRequest("Cannot delete category with associated items");
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted category with ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category with ID: {Id}", id);
                return StatusCode(500, "An error occurred while deleting the category");
            }
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
} 