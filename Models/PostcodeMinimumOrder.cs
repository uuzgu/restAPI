using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantApi.Models
{
    [Table("PostcodeMinimumOrders")]
    public class PostcodeMinimumOrder
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Required]
        [Column("Postcode")]
        public string Postcode { get; set; }
        
        [Required]
        [Column("MinimumOrderValue")]
        public decimal MinimumOrderValue { get; set; }
    }
} 