using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantApi.Models
{
    [Table("category_selection_groups")]
    public class CategorySelectionGroup
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("category_id")]
        public int CategoryId { get; set; }

        [Required]
        [Column("selection_group_id")]
        public int SelectionGroupId { get; set; }

        // Navigation properties
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        [ForeignKey("SelectionGroupId")]
        public SelectionGroup SelectionGroup { get; set; }
    }
} 