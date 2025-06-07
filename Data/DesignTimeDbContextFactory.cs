using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace RestaurantApi.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<RestaurantContext>
    {
        public RestaurantContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RestaurantContext>();
            // Use your actual connection string here
            optionsBuilder.UseSqlite("Data Source=restaurant.db");
            return new RestaurantContext(optionsBuilder.Options);
        }
    }
} 