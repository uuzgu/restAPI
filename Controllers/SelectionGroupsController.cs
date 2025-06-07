using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.Models;

namespace RestaurantApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SelectionGroupsController : ControllerBase
    {
        private readonly RestaurantContext _context;
        private readonly ILogger<SelectionGroupsController> _logger;

        public SelectionGroupsController(RestaurantContext context, ILogger<SelectionGroupsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/SelectionGroups
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SelectionGroup>>> GetSelectionGroups()
        {
            try
            {
                _logger.LogInformation("Fetching all selection groups");
                var selectionGroups = await _context.SelectionGroups
                    .Include(sg => sg.SelectionOptions)
                    .Include(sg => sg.ItemSelectionGroups)
                        .ThenInclude(isg => isg.Item)
                    .Include(sg => sg.CategorySelectionGroups)
                        .ThenInclude(csg => csg.Category)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} selection groups", selectionGroups.Count);
                return Ok(selectionGroups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching selection groups");
                return StatusCode(500, "An error occurred while fetching selection groups");
            }
        }

        // GET: api/SelectionGroups/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SelectionGroup>> GetSelectionGroup(int id)
        {
            try
            {
                _logger.LogInformation("Fetching selection group with ID: {Id}", id);
                var selectionGroup = await _context.SelectionGroups
                    .Include(sg => sg.SelectionOptions)
                    .Include(sg => sg.ItemSelectionGroups)
                        .ThenInclude(isg => isg.Item)
                    .Include(sg => sg.CategorySelectionGroups)
                        .ThenInclude(csg => csg.Category)
                    .FirstOrDefaultAsync(sg => sg.Id == id);

                if (selectionGroup == null)
                {
                    _logger.LogWarning("Selection group with ID {Id} not found", id);
                    return NotFound();
                }

                _logger.LogInformation("Found selection group: {Name}", selectionGroup.Name);
                return selectionGroup;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching selection group with ID: {Id}", id);
                return StatusCode(500, "An error occurred while fetching the selection group");
            }
        }

        // POST: api/SelectionGroups
        [HttpPost]
        public async Task<ActionResult<SelectionGroup>> CreateSelectionGroup(SelectionGroup selectionGroup)
        {
            try
            {
                if (string.IsNullOrEmpty(selectionGroup.Name))
                {
                    return BadRequest("Selection group name is required");
                }

                if (string.IsNullOrEmpty(selectionGroup.Type))
                {
                    return BadRequest("Selection group type is required");
                }

                if (selectionGroup.MinSelect < 0)
                {
                    return BadRequest("Minimum select must be greater than or equal to 0");
                }

                if (selectionGroup.MaxSelect < selectionGroup.MinSelect)
                {
                    return BadRequest("Maximum select must be greater than or equal to minimum select");
                }

                _logger.LogInformation("Creating new selection group: {Name}", selectionGroup.Name);
                _context.SelectionGroups.Add(selectionGroup);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created selection group with ID: {Id}", selectionGroup.Id);
                return CreatedAtAction(nameof(GetSelectionGroup), new { id = selectionGroup.Id }, selectionGroup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating selection group");
                return StatusCode(500, "An error occurred while creating the selection group");
            }
        }

        // PUT: api/SelectionGroups/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSelectionGroup(int id, SelectionGroup selectionGroup)
        {
            try
            {
                if (id != selectionGroup.Id)
                {
                    return BadRequest();
                }

                if (string.IsNullOrEmpty(selectionGroup.Name))
                {
                    return BadRequest("Selection group name is required");
                }

                if (string.IsNullOrEmpty(selectionGroup.Type))
                {
                    return BadRequest("Selection group type is required");
                }

                if (selectionGroup.MinSelect < 0)
                {
                    return BadRequest("Minimum select must be greater than or equal to 0");
                }

                if (selectionGroup.MaxSelect < selectionGroup.MinSelect)
                {
                    return BadRequest("Maximum select must be greater than or equal to minimum select");
                }

                _logger.LogInformation("Updating selection group with ID: {Id}", id);
                _context.Entry(selectionGroup).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SelectionGroupExists(id))
                    {
                        _logger.LogWarning("Selection group with ID {Id} not found", id);
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                _logger.LogInformation("Updated selection group with ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating selection group with ID: {Id}", id);
                return StatusCode(500, "An error occurred while updating the selection group");
            }
        }

        // DELETE: api/SelectionGroups/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSelectionGroup(int id)
        {
            try
            {
                _logger.LogInformation("Deleting selection group with ID: {Id}", id);
                var selectionGroup = await _context.SelectionGroups
                    .Include(sg => sg.ItemSelectionGroups)
                    .Include(sg => sg.CategorySelectionGroups)
                    .FirstOrDefaultAsync(sg => sg.Id == id);

                if (selectionGroup == null)
                {
                    _logger.LogWarning("Selection group with ID {Id} not found", id);
                    return NotFound();
                }

                if (selectionGroup.ItemSelectionGroups.Any() || selectionGroup.CategorySelectionGroups.Any())
                {
                    return BadRequest("Cannot delete selection group with associated items or categories");
                }

                _context.SelectionGroups.Remove(selectionGroup);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted selection group with ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting selection group with ID: {Id}", id);
                return StatusCode(500, "An error occurred while deleting the selection group");
            }
        }

        private bool SelectionGroupExists(int id)
        {
            return _context.SelectionGroups.Any(e => e.Id == id);
        }
    }
} 