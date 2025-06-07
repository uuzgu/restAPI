using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RestaurantApi.Models
{
    public class StripeCheckoutItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

        [JsonPropertyName("selectedItems")]
        public List<SelectedItem>? SelectedItems { get; set; }

        [JsonPropertyName("selectionGroups")]
        public List<SelectionGroupWithOptions>? SelectionGroups { get; set; }

        [JsonPropertyName("categorySelectionGroups")]
        public List<SelectionGroupWithOptions>? CategorySelectionGroups { get; set; }

        [JsonPropertyName("groupOrder")]
        public List<string>? GroupOrder { get; set; }

        [JsonPropertyName("image")]
        public string? Image { get; set; }
    }

    public class SelectedItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("groupName")]
        public string? GroupName { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }
}
