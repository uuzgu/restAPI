using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantApi.Models
{
    [Table("coupons_history")]
    public class CouponHistory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("coupon_id")]
        public int CouponId { get; set; }

        [ForeignKey("CouponId")]
        public Coupon Coupon { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("used_at")]
        public DateTime? UsedAt { get; set; } = DateTime.UtcNow;
    }
} 