using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantApi.Models
{
    [Table("delivery_addresses")]
    public class DeliveryAddress
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("postcode_id")]
        public int PostcodeId { get; set; }

        [Required]
        [Column("street")]
        public string Street { get; set; }

        [Column("house")]
        public string? House { get; set; }

        [Column("stairs")]
        public string? Stairs { get; set; }

        [Column("stick")]
        public string? Stick { get; set; }

        [Column("door")]
        public string? Door { get; set; }

        [Column("bell")]
        public string? Bell { get; set; }

        // Navigation properties
        [ForeignKey("PostcodeId")]
        public virtual Postcode Postcode { get; set; } = null!;
        public virtual Order? Order { get; set; }
    }
} 