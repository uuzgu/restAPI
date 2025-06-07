using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace RestaurantApi.Models;

[Table("order_details")]
public class OrderDetail
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("order_id")]
    public int OrderId { get; set; }

    [Required]
    [Column("item_details")]
    public string ItemDetails { get; set; }

    [NotMapped]
    public ItemOptions? ItemDetailsObject
    {
        get => string.IsNullOrEmpty(ItemDetails) ? null : JsonSerializer.Deserialize<ItemOptions>(ItemDetails);
        set => ItemDetails = value == null ? string.Empty : JsonSerializer.Serialize(value);
    }

    // Navigation property
    [ForeignKey("OrderId")]
    public virtual Order Order { get; set; } = null!;
} 