using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.Models;

namespace RestaurantApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SelectionOptionsController : ControllerBase
    {
        private readonly RestaurantContext _context;
        private readonly ILogger<SelectionOptionsController> _logger;

        public SelectionOptionsController(RestaurantContext context, ILogger<SelectionOptionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/SelectionOptions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SelectionOption>>> GetSelectionOptions()
        {
            try
            {
                _logger.LogInformation("Fetching all selection options");
                var selectionOptions = await _context.SelectionOptions
                    .Include(so => so.SelectionGroup)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} selection options", selectionOptions.Count);
                return Ok(selectionOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching selection options");
                return StatusCode(500, "An error occurred while fetching selection options");
            }
        }

        // GET: api/SelectionOptions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SelectionOption>> GetSelectionOption(int id)
        {
            try
            {
                _logger.LogInformation("Fetching selection option with ID: {Id}", id);
                var selectionOption = await _context.SelectionOptions
                    .Include(so => so.SelectionGroup)
                    .FirstOrDefaultAsync(so => so.Id == id);

                if (selectionOption == null)
                {
                    _logger.LogWarning("Selection option with ID {Id} not found", id);
                    return NotFound();
                }

                _logger.LogInformation("Found selection option: {Name}", selectionOption.Name);
                return selectionOption;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching selection option with ID: {Id}", id);
                return StatusCode(500, "An error occurred while fetching the selection option");
            }
        }

        // GET: api/SelectionOptions/group/5
        [HttpGet("group/{groupId}")]
        public async Task<ActionResult<IEnumerable<SelectionOption>>> GetSelectionOptionsByGroup(int groupId)
        {
            try
            {
                _logger.LogInformation("Fetching selection options for group ID: {GroupId}", groupId);
                var selectionOptions = await _context.SelectionOptions
                    .Where(so => so.SelectionGroupId == groupId)
                    .OrderBy(so => so.DisplayOrder)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} selection options for group {GroupId}", selectionOptions.Count, groupId);
                return Ok(selectionOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching selection options for group ID: {GroupId}", groupId);
                return StatusCode(500, "An error occurred while fetching selection options");
            }
        }

        // POST: api/SelectionOptions
        [HttpPost]
        public async Task<ActionResult<SelectionOption>> CreateSelectionOption(SelectionOption selectionOption)
        {
            try
            {
                if (string.IsNullOrEmpty(selectionOption.Name))
                {
                    return BadRequest("Selection option name is required");
                }

                if (selectionOption.Price < 0)
                {
                    return BadRequest("Price must be greater than or equal to 0");
                }

                // Verify that the selection group exists
                var selectionGroup = await _context.SelectionGroups.FindAsync(selectionOption.SelectionGroupId);
                if (selectionGroup == null)
                {
                    return BadRequest("Invalid selection group ID");
                }

                _logger.LogInformation("Creating new selection option: {Name}", selectionOption.Name);
                _context.SelectionOptions.Add(selectionOption);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created selection option with ID: {Id}", selectionOption.Id);
                return CreatedAtAction(nameof(GetSelectionOption), new { id = selectionOption.Id }, selectionOption);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating selection option");
                return StatusCode(500, "An error occurred while creating the selection option");
            }
        }

        // PUT: api/SelectionOptions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSelectionOption(int id, SelectionOption selectionOption)
        {
            try
            {
                if (id != selectionOption.Id)
                {
                    return BadRequest();
                }

                if (string.IsNullOrEmpty(selectionOption.Name))
                {
                    return BadRequest("Selection option name is required");
                }

                if (selectionOption.Price < 0)
                {
                    return BadRequest("Price must be greater than or equal to 0");
                }

                // Verify that the selection group exists
                var selectionGroup = await _context.SelectionGroups.FindAsync(selectionOption.SelectionGroupId);
                if (selectionGroup == null)
                {
                    return BadRequest("Invalid selection group ID");
                }

                _logger.LogInformation("Updating selection option with ID: {Id}", id);
                _context.Entry(selectionOption).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SelectionOptionExists(id))
                    {
                        _logger.LogWarning("Selection option with ID {Id} not found", id);
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                _logger.LogInformation("Updated selection option with ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating selection option with ID: {Id}", id);
                return StatusCode(500, "An error occurred while updating the selection option");
            }
        }

        // DELETE: api/SelectionOptions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSelectionOption(int id)
        {
            try
            {
                _logger.LogInformation("Deleting selection option with ID: {Id}", id);
                var selectionOption = await _context.SelectionOptions.FindAsync(id);

                if (selectionOption == null)
                {
                    _logger.LogWarning("Selection option with ID {Id} not found", id);
                    return NotFound();
                }

                _context.SelectionOptions.Remove(selectionOption);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted selection option with ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting selection option with ID: {Id}", id);
                return StatusCode(500, "An error occurred while deleting the selection option");
            }
        }

        private bool SelectionOptionExists(int id)
        {
            return _context.SelectionOptions.Any(e => e.Id == id);
        }
    }
} 