using System.Text.Json.Serialization;

namespace RestaurantApi.Models
{
    public class PaymentSuccessResponse
    {
        [JsonPropertyName("orderNumber")]
        public string OrderNumber { get; set; } = string.Empty;
        
        [JsonPropertyName("total")]
        public decimal Total { get; set; }
        
        [JsonPropertyName("items")]
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        
        [JsonPropertyName("estimatedTime")]
        public DateTime EstimatedTime { get; set; }
    }
} 