using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantApi.Models
{
    [Table("selection_options")]
    public class SelectionOption
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [Column("price")]
        public decimal Price { get; set; }
        
        [Required]
        [Column("display_order")]
        public int DisplayOrder { get; set; }
        
        [Required]
        [Column("selection_group_id")]
        public int SelectionGroupId { get; set; }

        // Navigation property
        [ForeignKey("SelectionGroupId")]
        public virtual SelectionGroup SelectionGroup { get; set; } = null!;
    }
} 