using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantApi.Models
{
    [Table("customerOrder_info")]
    public class CustomerOrderInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Required]
        [Column("first_name")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [Column("last_name")]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [Column("email")]
        public string Email { get; set; } = string.Empty;
        
        [Column("phone")]
        public string? Phone { get; set; }
        
        [Column("comment")]
        public string? Comment { get; set; }
        
        [Required]
        [Column("create_date")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
} 