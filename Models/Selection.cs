using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantApi.Models
{
    [Table("selections")]
    public class Selection
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Column("selection_option_id")]
        public int SelectionOptionId { get; set; }
        
        [Column("order_item_id")]
        public int OrderItemId { get; set; }

        // Navigation properties
        public virtual SelectionOption SelectionOption { get; set; }
        public virtual OrderItem OrderItem { get; set; }
    }
} 