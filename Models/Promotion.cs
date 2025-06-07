using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace RestaurantApi.Models
{
    public class Promotion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public bool IsActive { get; set; }

        // Foreign key to Item
        public int? ItemId { get; set; }
        public Item Item { get; set; }

        [Required]
        public string DisplayName { get; set; }

        [Required]
        public decimal DisplayPrice { get; set; }

        [Required]
        public bool IsBundle { get; set; }

        public decimal? DiscountPercentage { get; set; }
    }
} 