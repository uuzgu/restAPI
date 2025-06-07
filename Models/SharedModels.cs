using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RestaurantApi.Models
{
    public class CustomerInfoRequest
    {
        [Required]
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [Required]
        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

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

        [JsonPropertyName("comment")]
        public string? Comment { get; set; }
    }

    public class OrderItemRequest
    {
        [Required]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [Required]
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [Required]
        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

        [JsonPropertyName("selectedOptions")]
        public List<int> SelectedOptions { get; set; } = new List<int>();

        [JsonPropertyName("selectedItems")]
        public List<SelectedItemRequest>? SelectedItems { get; set; }
    }

    public class SelectedItemRequest
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }
    }
} 