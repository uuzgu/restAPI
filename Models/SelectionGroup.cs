using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantApi.Models
{
    [Table("selection_groups")]
    public class SelectionGroup
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("type")]
        public string Type { get; set; } = string.Empty;

        [Required]
        [Column("is_required")]
        public bool IsRequired { get; set; }

        [Required]
        [Column("min_select")]
        public int MinSelect { get; set; }

        [Required]
        [Column("max_select")]
        public int MaxSelect { get; set; }

        [Required]
        [Column("threshold")]
        public int Threshold { get; set; }

        [Required]
        [Column("display_order")]
        public int DisplayOrder { get; set; }

        // Navigation properties
        public virtual ICollection<SelectionOption> SelectionOptions { get; set; } = new List<SelectionOption>();
        public virtual ICollection<ItemSelectionGroup> ItemSelectionGroups { get; set; } = new List<ItemSelectionGroup>();
        public virtual ICollection<CategorySelectionGroup> CategorySelectionGroups { get; set; } = new List<CategorySelectionGroup>();
    }
} 