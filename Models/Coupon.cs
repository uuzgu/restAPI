using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantApi.Models
{
    [Table("coupons")]
    public class Coupon
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("code")]
        public string Code { get; set; }

        [Required]
        [Column("type")]
        public string Type { get; set; }  // e.g., "PERCENTAGE", "FIXED_AMOUNT"

        [Required]
        [Column("is_periodic")]
        public bool IsPeriodic { get; set; }

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [Required]
        [Column("discount_ratio")]
        public decimal DiscountRatio { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Required]
        [Column("is_used")]
        public bool IsUsed { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual ICollection<CouponSchedule> Schedules { get; set; } = new List<CouponSchedule>();
    }
} 