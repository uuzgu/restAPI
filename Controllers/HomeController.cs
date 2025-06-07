using Microsoft.AspNetCore.Mvc;

namespace RestaurantApi.Controllers
{
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("/")]
        public IActionResult Index()
        {
            _logger.LogInformation("Root path accessed");
            return Ok(new
            {
                Status = "API is running",
                Version = "1.0",
                Endpoints = new[]
                {
                    "/api/health",
                    "/api/categories",
                    "/api/items",
                    "/api/orders"
                }
            });
        }
    }
} 