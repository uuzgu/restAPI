using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantApi.Models
{
    public class PostcodeAddress
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("postcode_id")]
        public int PostcodeId { get; set; }

        [Required]
        [Column("street")]
        public string Street { get; set; }

        // Navigation property
        [ForeignKey("PostcodeId")]
        public Postcode Postcode { get; set; }
    }
} 