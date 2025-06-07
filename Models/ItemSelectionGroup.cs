using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantApi.Models
{
    [Table("item_selection_groups")]
    public class ItemSelectionGroup
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("item_id")]
        public int ItemId { get; set; }

        [Required]
        [Column("selection_group_id")]
        public int SelectionGroupId { get; set; }

        // Navigation properties
        public virtual Item Item { get; set; }
        public virtual SelectionGroup SelectionGroup { get; set; }
    }
} 