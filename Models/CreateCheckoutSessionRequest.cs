using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RestaurantApi.Models
{
    public class CreateCheckoutSessionRequest
    {
        [Required]
        [JsonPropertyName("items")]
        public required List<StripeCheckoutItem> Items { get; set; }

        [Required]
        [JsonPropertyName("customerInfo")]
        public required CustomerInfoRequest CustomerInfo { get; set; }

        [Required]
        [JsonPropertyName("orderMethod")]
        public required string OrderMethod { get; set; }

        [Required]
        [JsonPropertyName("paymentMethod")]
        public required string PaymentMethod { get; set; }

        [Required]
        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("specialNotes")]
        public string? SpecialNotes { get; set; }

        [Required]
        [JsonPropertyName("totalAmount")]
        public required decimal TotalAmount { get; set; }
    }
} 