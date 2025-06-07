using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.Models;
using Stripe;
using Stripe.Checkout;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RestaurantApi.Controllers
{
    [ApiController]
    [Route("api/stripe")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly RestaurantContext _context;
        private readonly string _webhookSecret;
        private readonly ILogger<StripeWebhookController> _logger;
        private const string CHECKOUT_SESSION_COMPLETED = "checkout.session.completed";
        private const string CHECKOUT_SESSION_EXPIRED = "checkout.session.expired";

        public StripeWebhookController(RestaurantContext context, IConfiguration configuration, ILogger<StripeWebhookController> logger)
        {
            _context = context;
            _webhookSecret = configuration["Stripe:WebhookSecret"] ?? throw new ArgumentNullException(nameof(configuration), "Stripe:WebhookSecret is not configured");
            _logger = logger;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"].ToString();
            
            if (string.IsNullOrEmpty(signature))
            {
                _logger.LogWarning("No Stripe signature found in request headers");
                return BadRequest("No Stripe signature found");
            }
            
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    signature,
                    _webhookSecret
                );
                
                _logger.LogInformation("Received Stripe event: {EventType}", stripeEvent.Type);

                switch (stripeEvent.Type)
                {
                    case CHECKOUT_SESSION_COMPLETED:
                        var session = stripeEvent.Data.Object as Session;
                        if (session != null)
                        {
                            _logger.LogInformation("Processing completed checkout session: {SessionId}", session.Id);
                            _logger.LogInformation("Session metadata: {Metadata}", 
                                string.Join(", ", session.Metadata.Select(kv => $"{kv.Key}={kv.Value}")));
                            
                            if (session.Metadata.TryGetValue("orderId", out string? orderIdStr) && 
                                !string.IsNullOrEmpty(orderIdStr) &&
                                int.TryParse(orderIdStr, out int orderId))
                            {
                                _logger.LogInformation("Found order ID in metadata: {OrderId}", orderId);
                                var order = await _context.Orders.FindAsync(orderId);
                                if (order != null)
                                {
                                    order.Status = "paid";
                                    order.UpdatedAt = DateTime.UtcNow;
                                    await _context.SaveChangesAsync();
                                    _logger.LogInformation("Order {OrderNumber} marked as paid", order.OrderNumber);
                                }
                                else
                                {
                                    _logger.LogWarning("Order {OrderId} not found in database", orderId);
                                }
                            }
                            else
                            {
                                _logger.LogWarning("No valid order ID found in session metadata");
                            }
                        }
                        break;

                    case CHECKOUT_SESSION_EXPIRED:
                        var expiredSession = stripeEvent.Data.Object as Session;
                        if (expiredSession != null)
                        {
                            _logger.LogInformation("Processing expired checkout session: {SessionId}", expiredSession.Id);
                            _logger.LogInformation("Session metadata: {Metadata}", 
                                string.Join(", ", expiredSession.Metadata.Select(kv => $"{kv.Key}={kv.Value}")));
                            
                            if (expiredSession.Metadata.TryGetValue("orderId", out string? expiredOrderIdStr) && 
                                !string.IsNullOrEmpty(expiredOrderIdStr) &&
                                int.TryParse(expiredOrderIdStr, out int expiredOrderId))
                            {
                                _logger.LogInformation("Found order ID in metadata: {OrderId}", expiredOrderId);
                                var order = await _context.Orders.FindAsync(expiredOrderId);
                                if (order != null)
                                {
                                    order.Status = "expired";
                                    order.UpdatedAt = DateTime.UtcNow;
                                    await _context.SaveChangesAsync();
                                    _logger.LogInformation("Order {OrderNumber} marked as expired", order.OrderNumber);
                                }
                                else
                                {
                                    _logger.LogWarning("Order {OrderId} not found in database", expiredOrderId);
                                }
                            }
                            else
                            {
                                _logger.LogWarning("No valid order ID found in session metadata");
                            }
                        }
                        break;

                    default:
                        _logger.LogWarning("Unhandled event type: {EventType}", stripeEvent.Type);
                        break;
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error");
                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                return StatusCode(500);
            }
        }
    }
} 