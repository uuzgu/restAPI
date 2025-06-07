using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantApi.Models
{
    [Table("item_allergens")]
    public class ItemAllergen
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("item_id")]
        public int ItemId { get; set; }

        [Required]
        [Column("allergen_code")]
        public string AllergenCode { get; set; }

        // Navigation property
        public virtual Item Item { get; set; }
    }
} 