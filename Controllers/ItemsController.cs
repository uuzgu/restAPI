using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.Models;
using System.Linq;

namespace RestaurantApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly RestaurantContext _context;
        private readonly ILogger<ItemsController> _logger;

        public ItemsController(RestaurantContext context, ILogger<ItemsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/items
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> GetItems()
        {
            try
            {
                _logger.LogInformation("Fetching all items");
                var items = await _context.Items
                    .Include(i => i.Category)
                    .Include(i => i.ItemAllergens)
                    .Include(i => i.ItemSelectionGroups)
                        .ThenInclude(isg => isg.SelectionGroup)
                            .ThenInclude(sg => sg.SelectionOptions)
                    .ToListAsync();
                
                _logger.LogInformation("Found {Count} items", items.Count);
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching items");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/items/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Item>> GetItem(int id)
        {
            try
            {
                _logger.LogInformation("Fetching item with ID: {Id}", id);
                var item = await _context.Items
                    .Include(i => i.Category)
                    .Include(i => i.ItemAllergens)
                    .Include(i => i.ItemSelectionGroups)
                        .ThenInclude(isg => isg.SelectionGroup)
                            .ThenInclude(sg => sg.SelectionOptions)
                    .FirstOrDefaultAsync(i => i.Id == id);
                    
                if (item == null)
                {
                    _logger.LogWarning("Item with ID {Id} not found", id);
                    return NotFound();
                }

                _logger.LogInformation("Found item: {Item}", item.Name);
                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching item with ID: {Id}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/items/{id}/options
        [HttpGet("{id}/options")]
        public async Task<ActionResult<ItemOptions>> GetItemOptions(int id)
        {
            try
            {
                _logger.LogInformation("Fetching options for item with ID: {Id}", id);
                // Check if the item exists and include all related data
                var item = await _context.Items
                    .Include(i => i.ItemAllergens)
                    .Include(i => i.ItemSelectionGroups)
                        .ThenInclude(isg => isg.SelectionGroup)
                            .ThenInclude(sg => sg.SelectionOptions)
                    .Include(i => i.Category)
                        .ThenInclude(c => c.CategorySelectionGroups)
                            .ThenInclude(csg => csg.SelectionGroup)
                                .ThenInclude(sg => sg.SelectionOptions)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (item == null)
                {
                    _logger.LogWarning("Item with ID {Id} not found", id);
                    return NotFound($"Item with ID {id} not found");
                }

                _logger.LogInformation("Found item: {Item}", item.Name);

                // Create item-specific selection groups
                var itemSelectionGroups = item.ItemSelectionGroups
                    .OrderBy(isg => isg.SelectionGroup.DisplayOrder)
                    .Select(isg => new SelectionGroupWithOptions
                    {
                        Id = isg.SelectionGroup.Id,
                        Name = isg.SelectionGroup.Name,
                        Type = isg.SelectionGroup.Type,
                        IsRequired = isg.SelectionGroup.IsRequired,
                        MinSelect = isg.SelectionGroup.MinSelect,
                        MaxSelect = isg.SelectionGroup.MaxSelect,
                        Threshold = isg.SelectionGroup.Threshold,
                        DisplayOrder = isg.SelectionGroup.DisplayOrder,
                        Options = isg.SelectionGroup.SelectionOptions
                            .OrderBy(o => o.DisplayOrder)
                            .Select(o => new SelectionOption
                            {
                                Id = o.Id,
                                Name = o.Name,
                                Price = o.Price,
                                DisplayOrder = o.DisplayOrder,
                                SelectionGroupId = o.SelectionGroupId
                            }).ToList()
                    }).ToList();

                // Create category-level selection groups
                var categorySelectionGroups = item.Category.CategorySelectionGroups
                    .OrderBy(csg => csg.SelectionGroup.DisplayOrder)
                    .Select(csg => new SelectionGroupWithOptions
                    {
                        Id = csg.SelectionGroup.Id,
                        Name = csg.SelectionGroup.Name,
                        Type = csg.SelectionGroup.Type,
                        IsRequired = csg.SelectionGroup.IsRequired,
                        MinSelect = csg.SelectionGroup.MinSelect,
                        MaxSelect = csg.SelectionGroup.MaxSelect,
                        Threshold = csg.SelectionGroup.Threshold,
                        DisplayOrder = csg.SelectionGroup.DisplayOrder,
                        Options = csg.SelectionGroup.SelectionOptions
                            .OrderBy(o => o.DisplayOrder)
                            .Select(o => new SelectionOption
                            {
                                Id = o.Id,
                                Name = o.Name,
                                Price = o.Price,
                                DisplayOrder = o.DisplayOrder,
                                SelectionGroupId = o.SelectionGroupId
                            }).ToList()
                    }).ToList();

                // Combine both lists, preferring item-specific groups over category groups
                var combinedGroups = itemSelectionGroups
                    .Concat(categorySelectionGroups.Where(cg => !itemSelectionGroups.Any(ig => ig.Id == cg.Id)))
                    .OrderBy(g => g.DisplayOrder)
                    .ToList();

                // Create a new ItemOptions object with combined selection groups
                var itemOptions = new ItemOptions
                {
                    SelectionGroups = combinedGroups,
                    CategorySelectionGroups = new List<SelectionGroupWithOptions>() // Empty list since we combined everything
                };

                _logger.LogInformation("Successfully retrieved options for item: {Item}", item.Name);
                return Ok(itemOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching options for item with ID: {Id}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/items/types
        [HttpGet("types")]
        public async Task<ActionResult<IEnumerable<CategoryWithItemCount>>> GetItemsGroupedByType()
        {
            try
            {
                _logger.LogInformation("Fetching items grouped by type");
                var itemsGroupedByType = await _context.Items
                    .Include(i => i.Category)
                    .GroupBy(item => item.Category.Name)
                    .Select(group => new CategoryWithItemCount
                    {
                        Category = group.Key,
                        ItemCount = group.Count()
                    })
                    .ToListAsync();

                _logger.LogInformation("Found {Count} item types", itemsGroupedByType.Count);
                return Ok(itemsGroupedByType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching items grouped by type");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    // Helper class to return category and item count
    public class CategoryWithItemCount
    {
        public string? Category { get; set; }
        public int ItemCount { get; set; }
    }
}
