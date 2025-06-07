using Microsoft.AspNetCore.Mvc;

namespace RestaurantApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult HandleException(Exception ex)
        {
            if (ex is Stripe.StripeException)
            {
                return StatusCode(500, $"Stripe error: {ex.Message}");
            }
            
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }

        protected IActionResult HandleNotFound(string message)
        {
            return NotFound(message);
        }

        protected IActionResult HandleBadRequest(string message)
        {
            return BadRequest(message);
        }
    }
} 