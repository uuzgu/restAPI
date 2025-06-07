using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantApi.Models
{
    [Table("items")]
    public class Item
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;
        
        [Column("description")]
        public string? Description { get; set; }
        
        [Required]
        [Column("price")]
        public decimal Price { get; set; }
        
        [Required]
        [Column("category_id")]
        public int CategoryId { get; set; }
        
        [Column("image_url")]
        public string? ImageUrl { get; set; }

        // Navigation properties
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<ItemAllergen> ItemAllergens { get; set; } = new List<ItemAllergen>();
        public virtual ICollection<ItemSelectionGroup> ItemSelectionGroups { get; set; } = new List<ItemSelectionGroup>();
        public virtual ICollection<ItemOffer> ItemOffers { get; set; } = new List<ItemOffer>();
    }
}
