using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantApi.Models
{
    public class StreetAddress
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("postcode_id")]
        public int PostcodeId { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; }

        // Navigation property
        [ForeignKey("PostcodeId")]
        public Postcode Postcode { get; set; }
    }
} 