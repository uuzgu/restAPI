using Microsoft.EntityFrameworkCore;
using RestaurantApi.Models;

namespace RestaurantApi.Data
{
    public class RestaurantContext : DbContext
    {
        public RestaurantContext(DbContextOptions<RestaurantContext> options)
            : base(options)
        { }

        public DbSet<Item> Items { get; set; }
      
        public DbSet<Category> Categories { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<ItemOffer> ItemOffers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<CustomerOrderInfo> CustomerOrderInfos { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<ItemAllergen> ItemAllergens { get; set; }
        public DbSet<SelectionGroup> SelectionGroups { get; set; }
        public DbSet<SelectionOption> SelectionOptions { get; set; }
        public DbSet<ItemSelectionGroup> ItemSelectionGroups { get; set; }
        public DbSet<CategorySelectionGroup> CategorySelectionGroups { get; set; }
        public DbSet<Postcode> Postcodes { get; set; }
        public DbSet<DeliveryAddress> DeliveryAddresses { get; set; }
        public DbSet<PostcodeAddress> PostcodeAddresses { get; set; }
        public DbSet<PostcodeMinimumOrder> PostcodeMinimumOrders { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<CouponHistory> CouponHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure CouponHistory entity
            modelBuilder.Entity<CouponHistory>()
                .HasOne(ch => ch.Coupon)
                .WithMany()
                .HasForeignKey(ch => ch.CouponId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Order entity
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");
                entity.HasKey(e => e.Id).HasName("id");
                entity.Property(e => e.OrderNumber).HasColumnName("order_number").IsRequired();
                entity.Property(e => e.Status).HasColumnName("status").IsRequired();
                entity.Property(e => e.Total).HasColumnName("total").IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired();
                entity.Property(e => e.PaymentMethod).HasColumnName("payment_method").IsRequired();
                entity.Property(e => e.OrderMethod).HasColumnName("order_method").IsRequired();
                entity.Property(e => e.SpecialNotes).HasColumnName("special_notes");
                entity.Property(e => e.StripeSessionId).HasColumnName("stripe_session_id");
                entity.HasMany(e => e.OrderDetails)
                    .WithOne(d => d.Order)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure OrderDetail entity
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("order_details");
                entity.HasKey(e => e.Id).HasName("id");
                entity.Property(e => e.OrderId).HasColumnName("order_id").IsRequired();
                entity.Property(e => e.ItemDetails).HasColumnName("item_details").IsRequired();
                entity.HasOne(e => e.Order)
                    .WithMany(o => o.OrderDetails)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Item entity
            modelBuilder.Entity<Item>(entity =>
            {
                entity.ToTable("items");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasColumnName("name").IsRequired();
                entity.Property(e => e.Price).HasColumnName("price").IsRequired();
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.ImageUrl).HasColumnName("image_url");
                entity.Property(e => e.CategoryId).HasColumnName("category_id").IsRequired();
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Items)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.ItemAllergens)
                    .WithOne(e => e.Item)
                    .HasForeignKey(e => e.ItemId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Category entity
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");
                entity.HasKey(e => e.Id).HasName("id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            });

            // Configure Offer entity
            modelBuilder.Entity<Offer>(entity =>
            {
                entity.ToTable("offers");
                entity.HasKey(e => e.Id).HasName("Id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired();
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.DiscountPercentage).HasColumnName("discount_percentage").IsRequired();
                entity.Property(e => e.StartDate).HasColumnName("start_date").IsRequired();
                entity.Property(e => e.EndDate).HasColumnName("end_date").IsRequired();
                entity.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();
            });

            // Configure ItemOffer entity
            modelBuilder.Entity<ItemOffer>(entity =>
            {
                entity.ToTable("item_offers");
                entity.HasKey(e => e.Id).HasName("Id");
                entity.Property(e => e.ItemId).HasColumnName("item_id");
                entity.Property(e => e.OfferId).HasColumnName("offer_id");
                entity.HasOne(e => e.Item)
                    .WithMany(i => i.ItemOffers)
                    .HasForeignKey(e => e.ItemId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Offer)
                    .WithMany(o => o.ItemOffers)
                    .HasForeignKey(e => e.OfferId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CustomerOrderInfo entity
            modelBuilder.Entity<CustomerOrderInfo>(entity =>
            {
                entity.ToTable("customerOrder_info");
                entity.HasKey(e => e.Id).HasName("id");
                entity.Property(e => e.FirstName).HasColumnName("first_name").IsRequired();
                entity.Property(e => e.LastName).HasColumnName("last_name").IsRequired();
                entity.Property(e => e.Email).HasColumnName("email").IsRequired();
                entity.Property(e => e.Phone).HasColumnName("phone");
                entity.Property(e => e.Comment).HasColumnName("comment");
                entity.Property(e => e.CreateDate).HasColumnName("create_date").IsRequired();
            });

            // Seed Offers
            modelBuilder.Entity<Offer>().HasData(
                new Offer
                {
                    Id = 1,
                    Name = "Lunch Special",
                    Description = "20% off all items during lunch hours",
                    DiscountPercentage = 20,
                    StartDate = DateTime.Parse("2024-01-01"),
                    EndDate = DateTime.Parse("2024-12-31"),
                    IsActive = true
                },
                new Offer
                {
                    Id = 2,
                    Name = "Happy Hour",
                    Description = "15% off drinks during happy hour",
                    DiscountPercentage = 15,
                    StartDate = DateTime.Parse("2024-01-01"),
                    EndDate = DateTime.Parse("2024-12-31"),
                    IsActive = true
                }
            );

           

           

            // Configure Promotion entity
      
            // Configure ItemAllergen entity
            modelBuilder.Entity<ItemAllergen>(entity =>
            {
                entity.ToTable("item_allergens");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ItemId).HasColumnName("item_id").IsRequired();
                entity.Property(e => e.AllergenCode).HasColumnName("allergen_code").IsRequired();
                entity.HasOne(e => e.Item)
                    .WithMany(i => i.ItemAllergens)
                    .HasForeignKey(e => e.ItemId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure SelectionGroup entity
            modelBuilder.Entity<SelectionGroup>(entity =>
            {
                entity.ToTable("selection_groups");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasColumnName("name").IsRequired();
                entity.Property(e => e.Type).HasColumnName("type").IsRequired();
                entity.Property(e => e.IsRequired).HasColumnName("is_required").IsRequired();
                entity.Property(e => e.MinSelect).HasColumnName("min_select").IsRequired();
                entity.Property(e => e.MaxSelect).HasColumnName("max_select").IsRequired();
                entity.Property(e => e.Threshold).HasColumnName("threshold").IsRequired();
                entity.Property(e => e.DisplayOrder).HasColumnName("display_order").IsRequired();
            });

            // Configure SelectionOption entity
            modelBuilder.Entity<SelectionOption>(entity =>
            {
                entity.ToTable("selection_options");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasColumnName("name").IsRequired();
                entity.Property(e => e.Price).HasColumnName("price").IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.SelectionGroupId).HasColumnName("selection_group_id").IsRequired();
                entity.Property(e => e.DisplayOrder).HasColumnName("display_order").IsRequired();

                entity.HasOne(e => e.SelectionGroup)
                    .WithMany(g => g.SelectionOptions)
                    .HasForeignKey(e => e.SelectionGroupId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ItemSelectionGroup entity
            modelBuilder.Entity<ItemSelectionGroup>(entity =>
            {
                entity.ToTable("item_selection_groups");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ItemId).HasColumnName("item_id").IsRequired();
                entity.Property(e => e.SelectionGroupId).HasColumnName("selection_group_id").IsRequired();
                
                entity.HasOne(e => e.Item)
                    .WithMany(i => i.ItemSelectionGroups)
                    .HasForeignKey(e => e.ItemId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.SelectionGroup)
                    .WithMany(g => g.ItemSelectionGroups)
                    .HasForeignKey(e => e.SelectionGroupId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CategorySelectionGroup entity
            modelBuilder.Entity<CategorySelectionGroup>(entity =>
            {
                entity.ToTable("category_selection_groups");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CategoryId).HasColumnName("category_id").IsRequired();
                entity.Property(e => e.SelectionGroupId).HasColumnName("selection_group_id").IsRequired();
                
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.CategorySelectionGroups)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.SelectionGroup)
                    .WithMany(g => g.CategorySelectionGroups)
                    .HasForeignKey(e => e.SelectionGroupId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Postcode entity
            modelBuilder.Entity<Postcode>(entity =>
            {
                entity.ToTable("postcodes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).HasColumnName("code").IsRequired();
                entity.Property(e => e.District).HasColumnName("district").IsRequired();
            });

            // Configure DeliveryAddress entity
            modelBuilder.Entity<DeliveryAddress>(entity =>
            {
                entity.ToTable("delivery_addresses");
                entity.HasKey(e => e.Id).HasName("id");
                entity.Property(e => e.PostcodeId).HasColumnName("postcode_id").IsRequired();
                entity.Property(e => e.Street).HasColumnName("street").IsRequired();
                entity.Property(e => e.House).HasColumnName("house");
                entity.Property(e => e.Stairs).HasColumnName("stairs");
                entity.Property(e => e.Stick).HasColumnName("stick");
                entity.Property(e => e.Door).HasColumnName("door");
                entity.Property(e => e.Bell).HasColumnName("bell");

                entity.HasOne(e => e.Postcode)
                    .WithMany(p => p.DeliveryAddresses)
                    .HasForeignKey(e => e.PostcodeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Order)
                    .WithOne(o => o.DeliveryAddress)
                    .HasForeignKey<Order>("delivery_address_id")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure PostcodeMinimumOrder entity
            modelBuilder.Entity<PostcodeMinimumOrder>(entity =>
            {
                entity.ToTable("PostcodeMinimumOrders");
                entity.HasKey(e => e.Id).HasName("id");
                entity.Property(e => e.Postcode).HasColumnName("Postcode").IsRequired();
                entity.Property(e => e.MinimumOrderValue).HasColumnName("MinimumOrderValue").IsRequired().HasColumnType("decimal(18,2)");
            });

            // Configure PostcodeAddress entity
            modelBuilder.Entity<PostcodeAddress>(entity =>
            {
                entity.ToTable("postcode_addresses");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PostcodeId).HasColumnName("postcode_id").IsRequired();
                entity.Property(e => e.Street).HasColumnName("street").IsRequired();

                entity.HasOne(e => e.Postcode)
                    .WithMany(p => p.PostcodeAddresses)
                    .HasForeignKey(e => e.PostcodeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SelectionGroup>()
                .HasMany(sg => sg.SelectionOptions)
                .WithOne(so => so.SelectionGroup)
                .HasForeignKey(so => so.SelectionGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SelectionGroup>()
                .Property(sg => sg.Name)
                .IsRequired();
                
            modelBuilder.Entity<SelectionOption>()
                .Property(so => so.Name)
                .IsRequired();
                
            modelBuilder.Entity<SelectionOption>()
                .Property(so => so.Price)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<SelectionGroup>()
                .HasMany(sg => sg.SelectionOptions)
                .WithOne(so => so.SelectionGroup)
                .HasForeignKey(so => so.SelectionGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
