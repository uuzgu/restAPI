using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantApi.Models
{
    [Table("offers")]
    public class Offer
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Required]
        [Column("name")]
        public string Name { get; set; }
        
        [Column("description")]
        public string? Description { get; set; }
        
        [Column("discount_percentage")]
        public decimal? DiscountPercentage { get; set; }
        
        [Column("discount_amount")]
        public decimal? DiscountAmount { get; set; }
        
        [Column("start_date")]
        public DateTime? StartDate { get; set; }
        
        [Column("end_date")]
        public DateTime? EndDate { get; set; }
        
        [Column("is_active")]
        public bool IsActive { get; set; }

        // Navigation property
        public virtual ICollection<ItemOffer> ItemOffers { get; set; }
    }
} 