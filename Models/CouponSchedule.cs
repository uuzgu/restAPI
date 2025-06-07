using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantApi.Models
{
    [Table("coupon_schedule")]
    public class CouponSchedule
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("coupon_id")]
        public int CouponId { get; set; }

        [ForeignKey("CouponId")]
        public Coupon Coupon { get; set; }

        [Required]
        [Column("monday")]
        public bool Monday { get; set; }

        [Required]
        [Column("tuesday")]
        public bool Tuesday { get; set; }

        [Required]
        [Column("wednesday")]
        public bool Wednesday { get; set; }

        [Required]
        [Column("thursday")]
        public bool Thursday { get; set; }

        [Required]
        [Column("friday")]
        public bool Friday { get; set; }

        [Required]
        [Column("saturday")]
        public bool Saturday { get; set; }

        [Required]
        [Column("sunday")]
        public bool Sunday { get; set; }

        [Required]
        [Column("begin_time")]
        public TimeSpan BeginTime { get; set; }

        [Required]
        [Column("end_time")]
        public TimeSpan EndTime { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
} 