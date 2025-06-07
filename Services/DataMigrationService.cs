using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.Models;

namespace RestaurantApi.Services
{
    public class DataMigrationService
    {
        private readonly RestaurantContext _context;
        private readonly ILogger<DataMigrationService> _logger;

        public DataMigrationService(RestaurantContext context, ILogger<DataMigrationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task MigrateDataAsync()
        {
            try
            {
                // Migrate selection options
                var selectionOptions = await _context.SelectionOptions.ToListAsync();
                foreach (var option in selectionOptions)
                {
                    option.DisplayOrder = 0;
                }
                
                await _context.SaveChangesAsync();
                _logger.LogInformation("Data migration completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during data migration");
                throw;
            }
        }
    }
} 