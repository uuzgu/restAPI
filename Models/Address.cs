using System.Text.Json.Serialization;

namespace RestaurantApi.Models
{
    public class Address
    {
        [JsonPropertyName("postalCode")]
        public string? PostalCode { get; set; }

        [JsonPropertyName("street")]
        public string? Street { get; set; }

        [JsonPropertyName("house")]
        public string? House { get; set; }

        [JsonPropertyName("stairs")]
        public string? Stairs { get; set; }

        [JsonPropertyName("stick")]
        public string? Stick { get; set; }

        [JsonPropertyName("door")]
        public string? Door { get; set; }

        [JsonPropertyName("bell")]
        public string? Bell { get; set; }
    }
} 